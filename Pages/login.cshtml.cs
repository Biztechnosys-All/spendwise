using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.DLL;
using Spendwise_WebApp.Models;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace Spendwise_WebApp.Pages
{
    [IgnoreAntiforgeryToken]
    public class loginModel : PageModel
    {

        private readonly Spendwise_WebApp.DLL.AppDbContext _context;
        private readonly JwtTokenService _jwtTokenService;
        private readonly IConfiguration _config;
        private readonly EmailSender _emailSender;

        public loginModel(Spendwise_WebApp.DLL.AppDbContext context, JwtTokenService jwtTokenService, IConfiguration config, EmailSender emailSender)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
            _config = config;
            _emailSender = emailSender;
        }

        [BindProperty]
        public new User User { get; set; } = default!;
        public User UserData { get; set; } = default!;
        public string ErrorMessage { get; set; } = string.Empty;
        //public bool IsEmailVerified { get; set; } = true;
        //public bool IsUserExists { get; set; } = true;
        //public bool InvalidPass { get; set; } = true;
        public void OnGet()
        {
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public async Task<IActionResult> OnPostLogin([FromBody] LoginRequest login)
        {
            if (string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Password))
            {
                return new JsonResult(new { success = false, message = "Email and Password are required." });
            }

            string hashedPassword;
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(login.Password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to a hexadecimal string
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                hashedPassword = sb.ToString();
            }
            var userData = await _context.Users.FirstOrDefaultAsync(x => x.Email == login.Email && x.IsActive == true && x.IsAdmin == false);

            if (userData == null)
            {
                return new JsonResult(new { success = false, message = "User is not registered." });
            }

            // Compare hashed passwords
            if (userData.Password != hashedPassword)
            {
                return new JsonResult(new { success = false, message = "Invalid email or password." });
            }
            else
            {
                if (!userData.IsEmailVerified)
                {
                    return new JsonResult(new { success = false, message = "Please verify your email before logging in." });
                }

                var options = CookieOptionsHelper.GetDefaultOptions();
                Response.Cookies.Append("UserName", userData.Forename + " " + userData.Surname, options);
                Response.Cookies.Append("UserEmail", userData.Email, options);

                HttpContext.Response.Cookies.Append("AuthToken", _jwtTokenService.GenerateJwtToken(login.Email), options);

                return new JsonResult(new { success = true, redirectUrl = Url.Page("./account") });
            }

        }

        public IActionResult OnGetLogout()
        {
            // Clear all cookies using C#
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            HttpContext.Response.Cookies.Delete("AuthToken");

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetForgotPassword(string customerEmail)
        {
            if (string.IsNullOrWhiteSpace(customerEmail))
            {
                return new JsonResult(false);  // Return false if company name is empty
            }

            string resetToken = Guid.NewGuid().ToString(); // Generate unique token
            string connectionString = _config.GetConnectionString("DefaultConnection") ?? "";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "UPDATE Users SET ResetToken = @Token, ResetTokenExpiry = @Expiry WHERE Email = @Email";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", customerEmail);
                    cmd.Parameters.AddWithValue("@Token", resetToken);
                    cmd.Parameters.AddWithValue("@Expiry", DateTime.UtcNow.AddHours(1)); // Token expires in 1 hour
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        ModelState.AddModelError("", "Email not found.");
                        return Page();
                    }
                }
            }

            // Step 2: Send reset email
            string resetLink = Url.Page("/ResetPassword",
                pageHandler: null,
                values: new { email = customerEmail, token = resetToken ?? "" },
                protocol: Request.Scheme) ?? "";

            //await SendResetEmail(customerEmail, resetLink ?? "");
            await _emailSender.SendEmailAsync(customerEmail, "Reset Your Password",
                $"Click the link to reset your password:  <a href='{resetLink}'>Reset Password</a>.");

            ViewData["Message"] = "A password reset link has been sent to your email.";
            return Page();

        }

        //private async Task SendResetEmail(string email, string resetLink)
        //{
        //    var smtpClient = new SmtpClient(_config["EmailSettings:SmtpServer"] ?? "")
        //    {
        //        Port = int.Parse(_config["EmailSettings:Port"] ?? ""),
        //        Credentials = new NetworkCredential(_config["EmailSettings:SenderEmail"], _config["EmailSettings:SenderPassword"] ?? ""),
        //        EnableSsl = true
        //    };

        //    var mailMessage = new MailMessage
        //    {
        //        From = new MailAddress(_config["EmailSettings:SenderEmail"] ?? ""),
        //        Subject = "Reset Your Password",
        //        Body = $"Click the link to reset your password: <a href='{HtmlEncoder.Default.Encode(resetLink)}'>Reset Password</a>",
        //        IsBodyHtml = true
        //    };
        //    mailMessage.To.Add(email);

        //    await smtpClient.SendMailAsync(mailMessage);
        //}

    }
}
