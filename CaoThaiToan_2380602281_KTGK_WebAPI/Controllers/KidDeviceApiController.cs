using CaoThaiToan_2380602281_KTGK_WebAPI.models;
using CaoThaiToan_2380602281_KTGK_WebAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaoThaiToan_2380602281_KTGK_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [Route("api/kiddevice")]
    [Route("api/device")]
    [ApiController]
    public class KidDeviceApiController : ControllerBase
    {
        private readonly IKidDeviceRepository _deviceRepo;

        public KidDeviceApiController(IKidDeviceRepository deviceRepo)
        {
            _deviceRepo = deviceRepo;
        }

        // Lấy danh sách thiết bị (hỗ trợ lọc danh mục & tìm kiếm)
        [HttpGet]
        public async Task<IActionResult> GetDevices([FromQuery] string? search, [FromQuery] int? categoryId)
        {
            var devices = await _deviceRepo.GetDevicesAsync(search, categoryId);
            return Ok(devices);
        }

        // Tìm kiếm theo tên thiết bị
        [HttpGet("search")]
        public async Task<IActionResult> SearchDevices([FromQuery] string? name)
        {
            var devices = await _deviceRepo.GetDevicesAsync(name);
            return Ok(devices);
        }

        // Tìm theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeviceById(int id)
        {
            var device = await _deviceRepo.GetDeviceByIdAsync(id);
            if (device == null) return NotFound(new { message = $"Không tìm thấy thiết bị có ID {id}." });
            return Ok(device);
        }

        // Tìm theo mã IMEI (hỗ trợ tính năng Quét mã QR/Barcode)
        [HttpGet("imei/{imei}")]
        public async Task<IActionResult> GetDeviceByImei(string imei)
        {
            var device = await _deviceRepo.GetDeviceByImeiAsync(imei);
            if (device == null) return NotFound(new { message = $"Không tìm thấy thiết bị có mã IMEI {imei}." });
            return Ok(device);
        }

        // Thêm thiết bị mới (Admin hoặc Staff)
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> AddDevice([FromBody] KidDevice device)
        {
            if (string.IsNullOrWhiteSpace(device.Name) || string.IsNullOrWhiteSpace(device.Imei))
                return BadRequest(new { message = "Tên thiết bị và mã IMEI là bắt buộc." });

            var existing = await _deviceRepo.GetDeviceByImeiAsync(device.Imei);
            if (existing != null)
                return BadRequest(new { message = "Mã IMEI này đã tồn tại trong hệ thống." });

            await _deviceRepo.AddDeviceAsync(device);
            return CreatedAtAction(nameof(GetDeviceById), new { id = device.Id }, device);
        }

        // Cập nhật thiết bị (Admin hoặc Staff)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateDevice(int id, [FromBody] KidDevice device)
        {
            if (id != device.Id)
                return BadRequest(new { message = "ID thiết bị không khớp." });

            await _deviceRepo.UpdateDeviceAsync(device);
            return NoContent();
        }

        // Xóa thiết bị (Chỉ Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            await _deviceRepo.DeleteDeviceAsync(id);
            return NoContent();
        }
    }
}
