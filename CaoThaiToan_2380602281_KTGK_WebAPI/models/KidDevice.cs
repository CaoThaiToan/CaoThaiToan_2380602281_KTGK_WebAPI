namespace CaoThaiToan_2380602281_KTGK_WebAPI.models
{
    public class KidDevice
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Imei { get; set; } = string.Empty; // Mã định danh duy nhất quét QR/Barcode
        public string? Brand { get; set; }
        public decimal Price { get; set; }
        public string? Image { get; set; }
        public string? Description { get; set; }
        public string? AgeRange { get; set; } // Phù hợp độ tuổi: "4-8 tuổi", "6-12 tuổi"
        public int BatteryLevel { get; set; } = 100; // Mức pin 0-100%
        public bool IsOnline { get; set; } = true;
        public string Status { get; set; } = "InStock"; // InStock, Active, Maintenance, SOS

        // Khóa ngoại đến Danh mục (SetNull khi xóa danh mục)
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
