using CaoThaiToan_2380602281_KTGK_WebAPI.models;
using Microsoft.EntityFrameworkCore;

namespace CaoThaiToan_2380602281_KTGK_WebAPI.Repositories
{
    public interface IChildProfileRepository
    {
        Task<IEnumerable<ChildProfile>> GetProfilesByParentAsync(string parentId);
        Task<ChildProfile?> GetProfileByIdAsync(int id);
        Task AddProfileAsync(ChildProfile profile);
        Task UpdateProfileAsync(ChildProfile profile);
        Task DeleteProfileAsync(int id);
        Task<ChildProfile?> BindDeviceAsync(string parentId, BindDeviceModel model);
    }

    public class ChildProfileRepository : IChildProfileRepository
    {
        private readonly ApplicationDbContext _context;

        public ChildProfileRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ChildProfile>> GetProfilesByParentAsync(string parentId)
        {
            return await _context.ChildProfiles
                .Include(c => c.Device)
                .ThenInclude(d => d!.Category)
                .Where(c => c.ParentId == parentId)
                .ToListAsync();
        }

        public async Task<ChildProfile?> GetProfileByIdAsync(int id)
        {
            return await _context.ChildProfiles
                .Include(c => c.Device)
                .ThenInclude(d => d!.Category)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddProfileAsync(ChildProfile profile)
        {
            _context.ChildProfiles.Add(profile);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProfileAsync(ChildProfile profile)
        {
            _context.Entry(profile).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProfileAsync(int id)
        {
            var profile = await _context.ChildProfiles.FindAsync(id);
            if (profile != null)
            {
                _context.ChildProfiles.Remove(profile);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ChildProfile?> BindDeviceAsync(string parentId, BindDeviceModel model)
        {
            // Tìm thiết bị theo mã IMEI (quét từ mã QR/Barcode)
            var device = await _context.KidDevices.FirstOrDefaultAsync(d => d.Imei.Trim() == model.Imei.Trim());
            if (device == null)
            {
                // Nếu chưa có thiết bị với IMEI này, tự động đăng ký thiết bị mới
                device = new KidDevice
                {
                    Name = $"Thiết bị của bé {model.ChildName}",
                    Imei = model.Imei.Trim(),
                    Brand = "KidsSafe",
                    Price = 1200000,
                    Image = "https://images.unsplash.com/photo-1510557880182-3d4d3cba35a5?w=500",
                    Description = "Điện thoại an toàn định vị GPS cho trẻ em",
                    AgeRange = "6-12 tuổi",
                    BatteryLevel = 95,
                    IsOnline = true,
                    Status = "Active"
                };
                _context.KidDevices.Add(device);
                await _context.SaveChangesAsync();
            }
            else
            {
                device.Status = "Active";
                await _context.SaveChangesAsync();
            }

            var profile = new ChildProfile
            {
                ChildName = model.ChildName,
                BirthYear = model.BirthYear,
                Gender = model.Gender,
                ParentId = parentId,
                DeviceId = device.Id,
                EmergencyPhone = model.EmergencyPhone,
                Avatar = model.Gender == "Nữ" 
                    ? "https://images.unsplash.com/photo-1544717305-2782549b5136?w=150" 
                    : "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150"
            };

            _context.ChildProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return await GetProfileByIdAsync(profile.Id);
        }
    }
}
