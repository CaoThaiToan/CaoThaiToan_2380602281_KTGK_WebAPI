using Microsoft.AspNetCore.Identity;

namespace CaoThaiToan_2380602281_KTGK_WebAPI.models
{
    public class User : IdentityUser
    {
        public string? FullName { get; set; }
        public string? UserRole { get; set; } // Admin, Staff, Parent
        public string? Avatar { get; set; }
        public string? OtpCode { get; set; }
        public DateTime? OtpExpiry { get; set; }
    }
}
