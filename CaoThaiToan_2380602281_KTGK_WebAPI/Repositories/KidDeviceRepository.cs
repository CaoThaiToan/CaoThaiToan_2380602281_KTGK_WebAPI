using CaoThaiToan_2380602281_KTGK_WebAPI.models;
using Microsoft.EntityFrameworkCore;

namespace CaoThaiToan_2380602281_KTGK_WebAPI.Repositories
{
    public interface IKidDeviceRepository
    {
        Task<IEnumerable<KidDevice>> GetDevicesAsync(string? search = null, int? categoryId = null);
        Task<KidDevice?> GetDeviceByIdAsync(int id);
        Task<KidDevice?> GetDeviceByImeiAsync(string imei);
        Task AddDeviceAsync(KidDevice device);
        Task UpdateDeviceAsync(KidDevice device);
        Task DeleteDeviceAsync(int id);
    }

    public class KidDeviceRepository : IKidDeviceRepository
    {
        private readonly ApplicationDbContext _context;

        public KidDeviceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<KidDevice>> GetDevicesAsync(string? search = null, int? categoryId = null)
        {
            var query = _context.KidDevices.Include(d => d.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower().Trim();
                query = query.Where(d => d.Name.ToLower().Contains(term) 
                                      || d.Imei.ToLower().Contains(term)
                                      || (d.Brand != null && d.Brand.ToLower().Contains(term)));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(d => d.CategoryId == categoryId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<KidDevice?> GetDeviceByIdAsync(int id)
        {
            return await _context.KidDevices.Include(d => d.Category).FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<KidDevice?> GetDeviceByImeiAsync(string imei)
        {
            return await _context.KidDevices.Include(d => d.Category).FirstOrDefaultAsync(d => d.Imei.Trim() == imei.Trim());
        }

        public async Task AddDeviceAsync(KidDevice device)
        {
            _context.KidDevices.Add(device);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDeviceAsync(KidDevice device)
        {
            _context.Entry(device).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDeviceAsync(int id)
        {
            var device = await _context.KidDevices.FindAsync(id);
            if (device != null)
            {
                _context.KidDevices.Remove(device);
                await _context.SaveChangesAsync();
            }
        }
    }
}
