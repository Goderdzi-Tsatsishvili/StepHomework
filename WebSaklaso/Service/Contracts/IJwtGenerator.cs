using WebSaklaso.Models.Auth;

namespace WebSaklaso.Service.Contracts
{
    public interface IJwtGenerator
    {
        string GenerateJwtToken(AppUser applicationUser, IEnumerable<string> roles);
        string GenerateRefreshToken();
    }
}
