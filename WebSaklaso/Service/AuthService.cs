using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebSaklaso.Data;
using WebSaklaso.Entities;
using WebSaklaso.Exceptions;
using WebSaklaso.Models.Auth;
using WebSaklaso.Service.Contracts;

namespace WebSaklaso.Service
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly IJwtGenerator _jwtTokenGenerator;
        private const string _adminRole = "Admin";
        private const string _supplierRole = "Supplier";
        private const string _confirmEmailTitle = "Email Confirm";

        public AuthService(
            AppDbContext db,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IMapper mapper,
            IEmailService emailService,
            IConfiguration config,
            IJwtGenerator jwtTokenGenerator)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _emailService = emailService;
            _config = config;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var existing = await _db.RefreshTokens.Include(x => x.User).FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (existing == null)
                throw new BadRequestException("invalid refresh token");

            if (!existing.IsActive)
                throw new UnauthorizedException(existing.IsExpired ? "refresh token has expired" : "refresh token has been revoked");

            existing.RevokedAt = DateTimeOffset.Now;

            var roles = await _userManager.GetRolesAsync(existing.User);
            var response = await GenerateTokenPairAsync(existing.User, roles);

            await _db.SaveChangesAsync();

            return response;
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var existing = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (existing == null)
                throw new BadRequestException("Invalid Refresh token");

            if (!existing.IsActive)
                throw new BadRequestException("Token is already inactive");

            existing.RevokedAt = DateTimeOffset.Now;
            await _db.SaveChangesAsync();
        }

        public async Task ConfirmEmailAsync(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                throw new BadRequestException("User id and token are required parameters for email confirmation");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new BadRequestException("User not found");

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
                throw new BadRequestException(result.Errors.First().Description);

            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.Now);
            await _userManager.ResetAccessFailedCountAsync(user);
        }

        public Task<string> RegisterAdminAsync(RegisterRequestDto registrationRequestDto, string accountConfirmationUrl)
        {
            return RegisterUserAsync(
                registrationRequestDto,
                _adminRole,
                accountConfirmationUrl);
        }

        public Task<string> RegisterSupplierAsync(RegisterRequestDto registerationRequestDto, string accountConfirmationUrl = null)
        {
            return RegisterUserAsync(
                registerationRequestDto,
                _supplierRole,
                accountConfirmationUrl);
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto)
        {
            var user = await _db.ApplicationUsers.FirstOrDefaultAsync(x => x.UserName.ToLower().Trim() == loginRequestDto.UserName.ToLower().Trim());

            if (user == null)
                throw new BadRequestException("User with provided credentials not found");

            if (!user.EmailConfirmed)
                throw new Exception("Unable to sign in with locked account, please activate your account first");

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);

            if (!isValid)
                throw new Exception("Username or Password is incorrect");

            var roles = await _userManager.GetRolesAsync(user);

            return await GenerateTokenPairAsync(user, roles);
        }

        //Helper methods
        private async Task<LoginResponseDto> GenerateTokenPairAsync(AppUser user, IList<string> roles)
        {
            var accessToken = _jwtTokenGenerator.GenerateJwtToken(user, roles);

            var refreshToken = new RefreshToken
            {
                Token = _jwtTokenGenerator.GenerateRefreshToken(),
                UserId = user.Id,
                CreatedAt = DateTimeOffset.Now,
                ExpiresAt = DateTimeOffset.Now.AddDays(int.Parse(_config["JWT:RefreshTokenExpiryDays"]))
            };

            _db.RefreshTokens.Add(refreshToken);
            await _db.SaveChangesAsync();

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }

        private static string BuildAccountConfirmationUrl(string accountConfirmationUrl, AppUser userToReturn, string token)
        {
            return $"{accountConfirmationUrl}" + 
                   $"?userId={Uri.EscapeDataString(userToReturn.Id)}" + 
                   $"&token={Uri.EscapeDataString(token)}";
        }

        private static string EmailConfirmationBody(string confirmationUrl)
        {
            return $@"
                <h2>Account Activation</h2>
                <p>Your administrator account has been created.</p>
                <p>Please click the link below to activate your account:</p>
                <p>
                    <a href=""{confirmationUrl}"">
                        Activate Account
                    </a>
                </p>";
        }

        private async Task EnsureRoleExistsAsync(string role)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));
        }

        private async Task<string> RegisterUserAsync(
            RegisterRequestDto registrationRequestDto,
            string role,
            string accountConfirmationUrl = null)
        {
            var user = _mapper.Map<AppUser>(registrationRequestDto);

            user.EmailConfirmed = false;
            user.LockoutEnabled = false;
            user.LockoutEnd = null;

            var result = await _userManager.CreateAsync(
                user,
                registrationRequestDto.Password);

            if (!result.Succeeded)
                throw new BadRequestException(result.Errors.First().Description);

            await EnsureRoleExistsAsync(role);

            await _userManager.AddToRoleAsync(user, role);

            var confirmationToken =
                await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmationUrl = BuildAccountConfirmationUrl(
                accountConfirmationUrl,
                user,
                confirmationToken);

            var emailResponse = await _emailService.Send(
                user.Email,
                _confirmEmailTitle,
                EmailConfirmationBody(confirmationUrl));

            if (!emailResponse.success)
                throw new InternalServerException(emailResponse.error.Message);

            return user.Id;
        }
    }
}
