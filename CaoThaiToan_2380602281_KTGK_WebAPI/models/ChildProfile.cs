namespace CaoThaiToan_2380602281_KTGK_WebAPI.models
{
    public class ChildProfile
    {
        public int Id { get; set; }
        public string ChildName { get; set; } = string.Empty;
        public int BirthYear { get; set; }
        public string Gender { get; set; } = "Nam";
        public string? Avatar { get; set; }
        public string ParentId { get; set; } = string.Empty; // User Id của phụ huynh

        public int? DeviceId { get; set; }
        public KidDevice? Device { get; set; }

        public string? EmergencyPhone { get; set; }

        // Tính năng mới: Vùng an toàn ảo (Geofencing)
        public string? SafeZoneName { get; set; } = "Trường Học Của Bé";
        public double? SafeZoneLat { get; set; } = 10.762622;
        public double? SafeZoneLng { get; set; } = 106.660172;
        public double? SafeZoneRadiusMeters { get; set; } = 300.0;
    }
}
