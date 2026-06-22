using Microsoft.AspNetCore.Identity;
using WebSaklaso.Entities;

namespace WebSaklaso.Models.Auth
{
    public class AppUser : IdentityUser
    {
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
