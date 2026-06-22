using WebSaklaso.Models.Auth;

namespace WebSaklaso.Service.Contracts
{
    public interface IAuthService
    {
        Task<string> RegisterAdminAsync(RegisterRequestDto registrationRequestDto, string accountConfirmationUrl = null);
        Task<string> RegisterSupplierAsync(RegisterRequestDto registerationRequestDto, string accountConfirmationUrl = null);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto);
        Task RevokeRefreshTokenAsync(string refreshToken);
        Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
        Task ConfirmEmailAsync(string userId, string token);
    }
}
