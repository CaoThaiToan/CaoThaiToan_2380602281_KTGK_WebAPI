using CaoThaiToan_2380602281_KTGK_WebAPI.models;
using CaoThaiToan_2380602281_KTGK_WebAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CaoThaiToan_2380602281_KTGK_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [Route("api/childprofile")]
    [ApiController]
    [Authorize]
    public class ChildProfileApiController : ControllerBase
    {
        private readonly IChildProfileRepository _childRepo;

        public ChildProfileApiController(IChildProfileRepository childRepo)
        {
            _childRepo = childRepo;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Name) ?? "default_parent";
        }

        // Lấy danh sách hồ sơ bé của phụ huynh hiện tại
        [HttpGet]
        public async Task<IActionResult> GetMyChildren()
        {
            var parentId = GetCurrentUserId();
            var profiles = await _childRepo.GetProfilesByParentAsync(parentId);
            return Ok(profiles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChildById(int id)
        {
            var profile = await _childRepo.GetProfileByIdAsync(id);
            if (profile == null) return NotFound(new { message = "Không tìm thấy hồ sơ trẻ em." });
            return Ok(profile);
        }

        // Ghép nối thiết bị cho bé thông qua Quét mã QR/Barcode (Tính năng thuyết trình e)
        [HttpPost("bind-device")]
        [HttpPost("{id}/bind-device")]
        public async Task<IActionResult> BindDevice([FromRoute] int? id, [FromBody] BindDeviceModel model)
        {
            if (!ModelState.IsValid && string.IsNullOrEmpty(model.Imei)) return BadRequest(ModelState);

            var parentId = GetCurrentUserId();
            if (id.HasValue && string.IsNullOrEmpty(model.ChildName))
            {
                var existing = await _childRepo.GetProfileByIdAsync(id.Value);
                if (existing != null) model.ChildName = existing.ChildName;
            }
            if (string.IsNullOrEmpty(model.ChildName)) model.ChildName = "Bé Yêu";

            var profile = await _childRepo.BindDeviceAsync(parentId, model);
            return Ok(new
            {
                status = true,
                message = $"Đã ghép nối thành công thiết bị {model.Imei} cho bé {model.ChildName}!",
                profile = profile,
                childName = profile.ChildName,
                deviceId = profile.DeviceId
            });
        }

        // Thêm hồ sơ bé thủ công
        [HttpPost]
        public async Task<IActionResult> AddProfile([FromBody] ChildProfile profile)
        {
            profile.ParentId = GetCurrentUserId();
            await _childRepo.AddProfileAsync(profile);
            return CreatedAtAction(nameof(GetChildById), new { id = profile.Id }, profile);
        }

        // Cập nhật hồ sơ bé (Cài đặt Vùng An Toàn Geofencing)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] ChildProfile profile)
        {
            if (id != profile.Id) return BadRequest(new { message = "ID hồ sơ không khớp." });

            profile.ParentId = GetCurrentUserId();
            await _childRepo.UpdateProfileAsync(profile);
            return NoContent();
        }

        // Xóa hồ sơ bé
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfile(int id)
        {
            await _childRepo.DeleteProfileAsync(id);
            return NoContent();
        }
    }
}
