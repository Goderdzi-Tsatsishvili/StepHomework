using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebSaklaso.Models.Auth;

namespace WebSaklaso.Entities
{
    public class RefreshToken
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(512)]
        public string Token { get; set; }

        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
        public AppUser User { get; set; }

        public DateTimeOffset ExpiresAt { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? RevokedAt { get; set; }

        public bool IsExpired => DateTimeOffset.Now >= ExpiresAt;
        public bool IsRevoked => RevokedAt != null;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
