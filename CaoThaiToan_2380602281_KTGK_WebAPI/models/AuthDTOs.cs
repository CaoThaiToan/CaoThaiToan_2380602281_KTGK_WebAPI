using System.ComponentModel.DataAnnotations;

namespace CaoThaiToan_2380602281_KTGK_WebAPI.models
{
    public class RegistrationModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public string? FullName { get; set; }

        public string Role { get; set; } = "Parent"; // Admin, Staff, Parent
    }

    public class LoginModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class VerifyOtpModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required, StringLength(6, MinimumLength = 6)]
        public string OtpCode { get; set; } = string.Empty;
    }

    public class BindDeviceModel
    {
        public string? ChildName { get; set; }

        [Required]
        public string Imei { get; set; } = string.Empty;

        public int BirthYear { get; set; } = 2018;

        public string Gender { get; set; } = "Nam";

        public string? EmergencyPhone { get; set; }
    }

    public class TelemetryUpdateModel
    {
        public int DeviceId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }
        public double NoiseDecibel { get; set; }
        public bool IsFallDetected { get; set; }
        public bool IsSosTriggered { get; set; }
        public string? SosMessage { get; set; }
    }
}
