using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CaoThaiToan_2380602281_KTGK_WebAPI.Repositories
{
    public interface IEmailService
    {
        Task<bool> SendOtpEmailAsync(string toEmail, string otpCode, string username);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string otpCode, string username)
        {
            var smtpSettings = _config.GetSection("SmtpSettings");
            var host = smtpSettings["Host"];
            var port = int.TryParse(smtpSettings["Port"], out int p) ? p : 587;
            var user = smtpSettings["User"];
            var pass = smtpSettings["Password"];
            var senderEmail = smtpSettings["SenderEmail"] ?? user;
            var senderName = smtpSettings["SenderName"] ?? "Kids Safe-Phone Support";

            // In log OTP để hỗ trợ kiểm thử ngay lập tức trên máy ảo / Swagger
            _logger.LogInformation($"[DEV OTP NOTIFICATION] User: {username} | Email: {toEmail} | OTP Code: {otpCode}");
            Console.WriteLine($"\n=======================================================");
            Console.WriteLine($"[EMAIL OTP SENT] Đến: {toEmail} | Tên: {username}");
            Console.WriteLine($"MÃ XÁC THỰC OTP: >>> {otpCode} <<<");
            Console.WriteLine($"=======================================================\n");

            // Nếu chưa cấu hình mật khẩu SMTP thật thì kết thúc mô phỏng thành công
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) || pass == "YOUR_SMTP_PASSWORD")
            {
                return true;
            }

            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(senderName, senderEmail));
                email.To.Add(new MailboxAddress(username, toEmail));
                email.Subject = "Mã xác thực đăng nhập (OTP) - Ứng dụng Quản lý Điện thoại Trẻ em";

                var builder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px; max-width: 500px;'>
                            <h2 style='color: #1976D2; text-align: center;'>🛡️ Kids Safe-Phone</h2>
                            <p>Xin chào <strong>{username}</strong>,</p>
                            <p>Mã xác thực OTP đăng nhập ứng dụng quản lý điện thoại trẻ em của bạn là:</p>
                            <div style='background-color: #E3F2FD; font-size: 32px; font-weight: bold; color: #0D47A1; text-align: center; padding: 15px; border-radius: 8px; letter-spacing: 6px;'>
                                {otpCode}
                            </div>
                            <p style='color: #666; font-size: 13px; margin-top: 15px;'>Mã này có hiệu lực trong vòng 5 phút. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
                        </div>"
                };
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                smtp.Timeout = 4000;
                await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(user, pass);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email SMTP thực tế");
                return true; // Vẫn cho phép thành công ở chế độ dev
            }
        }
    }
}
