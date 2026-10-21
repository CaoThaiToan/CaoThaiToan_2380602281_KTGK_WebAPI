using CaoThaiToan_2380602281_KTGK_WebAPI.models;
using CaoThaiToan_2380602281_KTGK_WebAPI.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình DbContext kết nối Supabase (PostgreSQL) hoặc SQL Server
var supabaseConn = builder.Configuration.GetConnectionString("SupabaseConnection")
                   ?? Environment.GetEnvironmentVariable("SUPABASE_CONNECTION")
                   ?? Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrWhiteSpace(supabaseConn) && !supabaseConn.Contains("YOUR_SUPABASE_PASSWORD", StringComparison.OrdinalIgnoreCase))
{
    var parsedPostgresConn = ConvertPostgresUrlToNpgsql(supabaseConn);
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(parsedPostgresConn));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// 2. Đăng ký ASP.NET Core Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. Đăng ký Dependency Injection Repositories & Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IKidDeviceRepository, KidDeviceRepository>();
builder.Services.AddScoped<IChildProfileRepository, ChildProfileRepository>();
builder.Services.AddScoped<ITelemetryRepository, TelemetryRepository>();

builder.Services.AddControllers();

// 4. Cấu hình Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 5. Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 6. Cấu hình JWT Bearer Authentication
var jwtSettings = builder.Configuration.GetSection("JWTKey");
var secret = jwtSettings["Secret"] ?? "AReallyLongSecretKeyForKidsSafePhoneJwtSigning2026CaoThaiToan2380602281!";
var key = Encoding.UTF8.GetBytes(secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["ValidIssuer"] ?? "KidsSafePhoneIssuer",
        ValidAudience = jwtSettings["ValidAudience"] ?? "KidsSafePhoneAudience",
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// 7. Tự động khởi tạo Roles và Dữ liệu mẫu (Seed Data)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Tự động kiểm tra và tạo toàn bộ bảng trong Supabase PostgreSQL hoặc SQL Server nếu chưa có
    await db.Database.EnsureCreatedAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    // Khởi tạo 3 Role
    var roles = new[] { "Admin", "Staff", "Parent" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Seed tài khoản Admin mặc định
    if (await userManager.FindByNameAsync("admin") == null)
    {
        var admin = new User
        {
            UserName = "admin",
            Email = "admin@kidssafe.com",
            FullName = "Quản trị viên Cao Thái Toàn",
            UserRole = "Admin"
        };
        await userManager.CreateAsync(admin, "Admin@123");
        await userManager.AddToRoleAsync(admin, "Admin");
    }

    // Seed tài khoản Nhân viên (Staff) mặc định
    if (await userManager.FindByNameAsync("staff1") == null)
    {
        var staff = new User
        {
            UserName = "staff1",
            Email = "staff1@kidssafe.com",
            FullName = "Kỹ thuật viên Nguyễn Văn An",
            UserRole = "Staff"
        };
        await userManager.CreateAsync(staff, "Staff@123");
        await userManager.AddToRoleAsync(staff, "Staff");
    }

    // Seed tài khoản Phụ huynh (Parent) mặc định
    if (await userManager.FindByNameAsync("parent1") == null)
    {
        var parent = new User
        {
            UserName = "parent1",
            Email = "parent1@gmail.com",
            FullName = "Phụ huynh Trần Thị Mai",
            UserRole = "Parent"
        };
        await userManager.CreateAsync(parent, "Parent@123");
        await userManager.AddToRoleAsync(parent, "Parent");
    }
    else
    {
        var existingParent = await userManager.FindByNameAsync("parent1");
        if (existingParent != null && existingParent.Email != "reretailmanagement@gmail.com")
        {
            existingParent.Email = "reretailmanagement@gmail.com";
            await userManager.UpdateAsync(existingParent);
        }
    }

    // Seed Danh mục thiết bị trẻ em
    if (!await db.Categories.AnyAsync())
    {
        var cat1 = new Category { Name = "Đồng hồ thông minh định vị", Description = "Đồng hồ định vị GPS có nghe gọi video 4G", Icon = "watch" };
        var cat2 = new Category { Name = "Điện thoại phím an toàn", Description = "Điện thoại phím số giới hạn danh bạ người thân", Icon = "phone" };
        var cat3 = new Category { Name = "Smartphone giám sát trẻ em", Description = "Smartphone cài sẵn hệ điều hành giới hạn nội dung", Icon = "smartphone" };
        var cat4 = new Category { Name = "Thẻ định vị & Phụ kiện GPS", Description = "Thẻ AirTag/SmartTag gắn balo, giày đi học", Icon = "tag" };
        db.Categories.AddRange(cat1, cat2, cat3, cat4);
        await db.SaveChangesAsync();

        // Seed Thiết bị trẻ em mẫu
        var dev1 = new KidDevice
        {
            Name = "Đồng hồ MyKID 4G Viettel",
            Imei = "867542049182736",
            Brand = "Viettel",
            Price = 1450000,
            Image = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=500",
            Description = "Định vị 3 chế độ GPS/Wifi/LBS, chống nước IP68, camera chụp lén bảo vệ con",
            AgeRange = "5-10 tuổi",
            BatteryLevel = 88,
            IsOnline = true,
            Status = "Active",
            CategoryId = cat1.Id
        };
        var dev2 = new KidDevice
        {
            Name = "Điện thoại Masstel Dreamy Phone",
            Imei = "358920194827104",
            Brand = "Masstel",
            Price = 890000,
            Image = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=500",
            Description = "Điện thoại trẻ em không truy cập internet, chỉ gọi điện thoại người thân",
            AgeRange = "6-12 tuổi",
            BatteryLevel = 92,
            IsOnline = true,
            Status = "Active",
            CategoryId = cat2.Id
        };
        var dev3 = new KidDevice
        {
            Name = "Vòng đeo định vị Wonlex KT24S",
            Imei = "990012345678901",
            Brand = "Wonlex",
            Price = 1190000,
            Image = "https://images.unsplash.com/photo-1508685096489-7aacd43bd3b1?w=500",
            Description = "Gọi video call sắc nét, pin 3 ngày, cảm biến đo nhịp tim và tiếng ồn môi trường",
            AgeRange = "4-9 tuổi",
            BatteryLevel = 76,
            IsOnline = true,
            Status = "Active",
            CategoryId = cat1.Id
        };
        db.KidDevices.AddRange(dev1, dev2, dev3);
        await db.SaveChangesAsync();

        // Seed hồ sơ bé của parent1
        var parentUser = await userManager.FindByNameAsync("parent1");
        if (parentUser != null)
        {
            var child = new ChildProfile
            {
                ChildName = "Bé Bo (Thái An)",
                BirthYear = 2018,
                Gender = "Nam",
                ParentId = parentUser.Id,
                DeviceId = dev1.Id,
                EmergencyPhone = "0909123456",
                Avatar = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150",
                SafeZoneName = "Trường Tiểu Học Hòa Bình",
                SafeZoneLat = 10.762622,
                SafeZoneLng = 106.660172,
                SafeZoneRadiusMeters = 300
            };
            db.ChildProfiles.Add(child);
            await db.SaveChangesAsync();

            // Seed dữ liệu vị trí GPS & tiếng ồn
            db.DeviceTelemetries.Add(new DeviceTelemetry
            {
                DeviceId = dev1.Id,
                Latitude = 10.762622,
                Longitude = 106.660172,
                Address = "Khuôn viên Trường Tiểu học, Quận 1, TP.HCM",
                NoiseDecibel = 52.4,
                IsNoiseAlert = false,
                IsFallDetected = false,
                IsSosTriggered = false,
                Timestamp = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
    }
}

// Kích hoạt Swagger UI cho cả Development và Cloud Production (để giảng viên test trực tiếp từ link Web)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kids Safe-Phone API v1");
    c.RoutePrefix = string.Empty; // Mở link gốc Render (/) là vào thẳng Swagger UI
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Hàm chuyển đổi URL Supabase (PostgreSQL URI) sang chuỗi kết nối chuẩn Npgsql
static string ConvertPostgresUrlToNpgsql(string connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString)) return connectionString;

    var trimmed = connectionString.Trim();
    if (trimmed.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
        trimmed.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var uri = new Uri(trimmed);
            var userInfo = uri.UserInfo.Split(':');
            var username = Uri.UnescapeDataString(userInfo[0]);
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');
            if (string.IsNullOrEmpty(database)) database = "postgres";

            return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
        }
        catch
        {
            return connectionString;
        }
    }

    if (!trimmed.Contains("SSL Mode", StringComparison.OrdinalIgnoreCase))
    {
        trimmed += ";SSL Mode=Require;Trust Server Certificate=true;";
    }

    return trimmed;
}
