using CaoThaiToan_2380602281_KTGK_WebAPI.models;
using Microsoft.EntityFrameworkCore;

namespace CaoThaiToan_2380602281_KTGK_WebAPI.Repositories
{
    public interface ITelemetryRepository
    {
        Task<DeviceTelemetry?> GetLatestTelemetryAsync(int deviceId);
        Task<IEnumerable<DeviceTelemetry>> GetTelemetryHistoryAsync(int deviceId, int limit = 20);
        Task<DeviceTelemetry> AddTelemetryAsync(TelemetryUpdateModel model);
    }

    public class TelemetryRepository : ITelemetryRepository
    {
        private readonly ApplicationDbContext _context;

        public TelemetryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DeviceTelemetry?> GetLatestTelemetryAsync(int deviceId)
        {
            return await _context.DeviceTelemetries
                .Where(t => t.DeviceId == deviceId)
                .OrderByDescending(t => t.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<DeviceTelemetry>> GetTelemetryHistoryAsync(int deviceId, int limit = 20)
        {
            return await _context.DeviceTelemetries
                .Where(t => t.DeviceId == deviceId)
                .OrderByDescending(t => t.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<DeviceTelemetry> AddTelemetryAsync(TelemetryUpdateModel model)
        {
            var telemetry = new DeviceTelemetry
            {
                DeviceId = model.DeviceId,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                Address = model.Address ?? "Vị trí gần trường học",
                NoiseDecibel = model.NoiseDecibel,
                IsNoiseAlert = model.NoiseDecibel > 80.0,
                IsFallDetected = model.IsFallDetected,
                IsSosTriggered = model.IsSosTriggered,
                SosMessage = model.SosMessage,
                Timestamp = DateTime.UtcNow
            };

            _context.DeviceTelemetries.Add(telemetry);

            // Cập nhật trạng thái của thiết bị nếu có SOS
            var device = await _context.KidDevices.FindAsync(model.DeviceId);
            if (device != null)
            {
                if (model.IsSosTriggered)
                {
                    device.Status = "SOS";
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                await _context.SaveChangesAsync();
            }

            return telemetry;
        }
    }
}
