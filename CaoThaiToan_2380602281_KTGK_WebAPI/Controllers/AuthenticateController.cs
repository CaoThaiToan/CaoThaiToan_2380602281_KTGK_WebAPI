using CaoThaiToan_2380602281_KTGK_WebAPI.models;
using CaoThaiToan_2380602281_KTGK_WebAPI.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CaoThaiToan_2380602281_KTGK_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthenticateController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _emailService = emailService;
        }

        // 1. Đăng ký tài khoản qua Email
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status400BadRequest, new { status = false, message = "Tên đăng nhập đã tồn tại!" });

            var emailExists = await _userManager.FindByEmailAsync(model.Email);
            if (emailExists != null)
                return StatusCode(StatusCodes.Status400BadRequest, new { status = false, message = "Email đã được đăng ký!" });

            var user = new User
            {
                UserName = model.Username,
                Email = model.Email,
                FullName = model.FullName ?? model.Username,
                UserRole = string.IsNullOrWhiteSpace(model.Role) ? "Parent" : model.Role
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return StatusCode(StatusCodes.Status500InternalServerError, new { status = false, message = $"Đăng ký thất bại: {errors}" });
            }

            // Gán Role nếu chưa tồn tại
            if (!await _roleManager.RoleExistsAsync(user.UserRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(user.UserRole));
            }
            await _userManager.AddToRoleAsync(user, user.UserRole);

            return Ok(new { status = true, message = "Đăng ký tài khoản thành công! Vui lòng đăng nhập." });
        }

        // 2. Bước 1 Đăng nhập: Tên đăng nhập + Mật khẩu -> Phát sinh mã OTP
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Cho phép đăng nhập bằng Username hoặc Email
            var user = await _userManager.FindByNameAsync(model.Username) ?? await _userManager.FindByEmailAsync(model.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized(new { status = false, message = "Tên đăng nhập hoặc mật khẩu không chính xác!" });
            }

            // Tạo mã OTP ngẫu nhiên 6 chữ số
            var random = new Random();
            var otpCode = random.Next(100000, 999999).ToString();
            user.OtpCode = otpCode;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(5); // Mã OTP có hạn 5 phút
            await _userManager.UpdateAsync(user);

            // Gửi OTP qua Email
            await _emailService.SendOtpEmailAsync(user.Email ?? $"{user.UserName}@gmail.com", otpCode, user.UserName ?? "User");

            return Ok(new
            {
                status = true,
                message = "Xác thực tài khoản thành công! Mã OTP đã được gửi đến email của bạn.",
                requireOtp = true,
                username = user.UserName,
                email = user.Email,
                // Trả về devOtp để sinh viên và giảng viên kiểm thử dễ dàng ngay trên máy ảo / Swagger
                devOtp = otpCode 
            });
        }

        // 2. Bước 2 Đăng nhập: Xác thực mã OTP 6 số để nhận JWT Token
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByNameAsync(model.Username) ?? await _userManager.FindByEmailAsync(model.Username);
            if (user == null)
            {
                return NotFound(new { status = false, message = "Không tìm thấy tài khoản người dùng!" });
            }

            if (string.IsNullOrEmpty(user.OtpCode) || user.OtpCode != model.OtpCode.Trim())
            {
                return BadRequest(new { status = false, message = "Mã OTP không chính xác. Vui lòng kiểm tra lại!" });
            }

            if (user.OtpExpiry == null || user.OtpExpiry < DateTime.UtcNow)
            {
                return BadRequest(new { status = false, message = "Mã OTP đã hết hạn. Vui lòng yêu cầu mã mới!" });
            }

            // Xóa mã OTP sau khi xác thực thành công
            user.OtpCode = null;
            user.OtpExpiry = null;
            await _userManager.UpdateAsync(user);

            // Lấy danh sách Roles
            var userRoles = await _userManager.GetRolesAsync(user);
            var primaryRole = userRoles.FirstOrDefault() ?? user.UserRole ?? "Parent";

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("FullName", user.FullName ?? user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = GenerateToken(authClaims);

            return Ok(new
            {
                status = true,
                message = "Đăng nhập thành công!",
                token = token,
                role = primaryRole,
                userId = user.Id,
                username = user.UserName,
                fullName = user.FullName
            });
        }

        private string GenerateToken(IEnumerable<Claim> claims)
        {
            var jwtSettings = _configuration.GetSection("JWTKey");
            var secret = jwtSettings["Secret"] ?? "AReallyLongSecretKeyForKidsSafePhoneJwtSigning2026!";
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(jwtSettings["TokenExpiryTimeInHour"] ?? "24")),
                Issuer = jwtSettings["ValidIssuer"] ?? "KidsSafePhoneIssuer",
                Audience = jwtSettings["ValidAudience"] ?? "KidsSafePhoneAudience",
                SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
