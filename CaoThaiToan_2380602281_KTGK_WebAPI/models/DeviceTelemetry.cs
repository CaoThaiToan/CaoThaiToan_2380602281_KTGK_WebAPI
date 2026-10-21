namespace CaoThaiToan_2380602281_KTGK_WebAPI.models
{
    public class DeviceTelemetry
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }
        public double NoiseDecibel { get; set; } // Mức độ tiếng ồn (dB)
        public bool IsNoiseAlert { get; set; } = false; // Tiếng ồn vượt ngưỡng nguy hiểm (>80dB)
        public bool IsFallDetected { get; set; } = false; // Phát hiện té ngã / va chạm mạnh
        public bool IsSosTriggered { get; set; } = false; // Bấm nút SOS khẩn cấp
        public string? SosMessage { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
