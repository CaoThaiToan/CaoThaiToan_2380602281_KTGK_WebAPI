using CaoThaiToan_2380602281_KTGK_WebAPI.models;
using CaoThaiToan_2380602281_KTGK_WebAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaoThaiToan_2380602281_KTGK_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [Route("api/telemetry")]
    [ApiController]
    public class TelemetryApiController : ControllerBase
    {
        private readonly ITelemetryRepository _telemetryRepo;

        public TelemetryApiController(ITelemetryRepository telemetryRepo)
        {
            _telemetryRepo = telemetryRepo;
        }

        // 1. Tính năng (b) Lấy tọa độ GPS và trạng thái hiện tại của thiết bị bé
        [HttpGet("device/{deviceId}/latest")]
        [HttpGet("latest/{deviceId}")]
        public async Task<IActionResult> GetLatestTelemetry(int deviceId)
        {
            var telemetry = await _telemetryRepo.GetLatestTelemetryAsync(deviceId);
            if (telemetry == null)
            {
                // Trả về dữ liệu mẫu mặc định nếu thiết bị chưa có tọa độ
                return Ok(new DeviceTelemetry
                {
                    DeviceId = deviceId,
                    Latitude = 10.762622,
                    Longitude = 106.660172,
                    Address = "Trường Tiểu học Chu Văn An, TP. Hồ Chí Minh",
                    NoiseDecibel = 45.2,
                    IsNoiseAlert = false,
                    IsFallDetected = false,
                    IsSosTriggered = false,
                    Timestamp = DateTime.UtcNow
                });
            }
            return Ok(telemetry);
        }

        // Lấy lịch sử di chuyển của bé
        [HttpGet("device/{deviceId}/history")]
        public async Task<IActionResult> GetTelemetryHistory(int deviceId, [FromQuery] int limit = 15)
        {
            var history = await _telemetryRepo.GetTelemetryHistoryAsync(deviceId, limit);
            return Ok(history);
        }

        // Cập nhật tọa độ GPS, mức đo tiếng ồn (dB), phát hiện té ngã
        [HttpPost]
        [HttpPost("update")]
        public async Task<IActionResult> UpdateTelemetry([FromBody] TelemetryUpdateModel model)
        {
            var result = await _telemetryRepo.AddTelemetryAsync(model);
            return Ok(new
            {
                status = true,
                message = "Cập nhật dữ liệu cảm biến thành công!",
                telemetry = result,
                noiseDecibel = result.NoiseDecibel,
                isFallDetected = result.IsFallDetected
            });
        }

        // Tính năng mới: Gửi tín hiệu Silent SOS khẩn cấp từ máy của bé
        [HttpPost("sos")]
        [HttpPost("device/{deviceId}/sos")]
        public async Task<IActionResult> TriggerSos([FromRoute] int? deviceId, [FromBody] TelemetryUpdateModel model)
        {
            if (deviceId.HasValue) model.DeviceId = deviceId.Value;
            model.IsSosTriggered = true;
            model.SosMessage = model.SosMessage ?? "🚨 KHẨN CẤP! Bé vừa nhấn nút SOS cần sự trợ giúp!";
            
            var result = await _telemetryRepo.AddTelemetryAsync(model);
            return Ok(new
            {
                status = true,
                alert = "🚨 CẢNH BÁO KHẨN CẤP SOS ĐÃ ĐƯỢC KÍCH HOẠT VÀ GỬI TỚI PHỤ HUYNH!",
                telemetry = result,
                isSosTriggered = result.IsSosTriggered,
                latitude = result.Latitude,
                longitude = result.Longitude,
                noiseDecibel = result.NoiseDecibel
            });
        }
    }
}
