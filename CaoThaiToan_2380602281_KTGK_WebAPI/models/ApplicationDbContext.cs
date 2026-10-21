using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CaoThaiToan_2380602281_KTGK_WebAPI.models
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<KidDevice> KidDevices { get; set; }
        public DbSet<ChildProfile> ChildProfiles { get; set; }
        public DbSet<DeviceTelemetry> DeviceTelemetries { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            if (Database.IsSqlServer())
            {
                builder.HasDefaultSchema("identity");
            }

            builder.Entity<KidDevice>(entity =>
            {
                entity.Property(p => p.Price).HasPrecision(18, 2);

                // Yêu cầu 8: Xóa danh mục thì thiết bị không bị xóa mà CategoryId trở về NULL (SetNull)
                entity.HasOne(d => d.Category)
                      .WithMany(c => c.Devices)
                      .HasForeignKey(d => d.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(d => d.Imei).IsUnique();
            });

            builder.Entity<ChildProfile>(entity =>
            {
                entity.HasOne(c => c.Device)
                      .WithMany()
                      .HasForeignKey(c => c.DeviceId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
