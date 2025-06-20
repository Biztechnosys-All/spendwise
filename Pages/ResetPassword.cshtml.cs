using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace Spendwise_WebApp.Pages
{
    public class ResetPasswordModel : PageModel
    {

        [BindProperty]
        [Required]
        public string Email { get; set; }

        [BindProperty]
        [Required]
        public string Token { get; set; }

        [BindProperty]
        [Required]
        public string NewPassword { get; set; }

        private readonly IConfiguration _config;

        public ResetPasswordModel(IConfiguration config)
        {
            _config = config;
        }


        public async Task<IActionResult> OnGetAsync(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                ViewData["ErrorMessage"] = "Invalid or expired link. Please request a new password reset.";
                return Page();
            }

            Email = email;
            Token = token;

            // Check if token exists and is valid
            string connectionString = _config.GetConnectionString("DefaultConnection") ?? "";
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT ResetToken, ResetTokenExpiry FROM Users WHERE Email = @Email";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", Email);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!reader.Read() || reader["ResetToken"].ToString() != Token)
                        {
                            ViewData["ErrorMessage"] = "Invalid or expired link. Please request a new password reset.";
                            return Page();
                        }

                        DateTime expiry = Convert.ToDateTime(reader["ResetTokenExpiry"]);
                        if (expiry < DateTime.UtcNow)
                        {
                            ViewData["ErrorMessage"] = "Your password reset link has expired. Please request a new one.";
                            return Page();
                        }
                    }
                }
            }

            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            string connectionString = _config.GetConnectionString("DefaultConnection") ?? "";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT ResetToken, ResetTokenExpiry FROM Users WHERE Email = @Email";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", Email);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!reader.Read())
                        {
                            ModelState.AddModelError("", "Invalid token.");
                            return Page();
                        }

                        string storedToken = reader["ResetToken"].ToString() ?? "";
                        DateTime expiry = Convert.ToDateTime(reader["ResetTokenExpiry"]);

                        if (storedToken != Token || expiry < DateTime.UtcNow)
                        {
                            ModelState.AddModelError("", "Invalid or expired token.");
                            return Page();
                        }
                    }
                }

                // Hash new password
                string hashedPassword = NewPassword;

                using (MD5 md5 = MD5.Create())
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(NewPassword);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    // Convert the byte array to a hexadecimal string
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("x2"));
                    }
                    hashedPassword = sb.ToString();
                }


                // Update password in DB
                string updateQuery = "UPDATE Users SET Password = @Password, ResetToken = NULL, ResetTokenExpiry = NULL WHERE Email = @Email";
                using (var updateCmd = new SqlCommand(updateQuery, conn))
                {
                    updateCmd.Parameters.AddWithValue("@Email", Email);
                    updateCmd.Parameters.AddWithValue("@Password", hashedPassword);
                    await updateCmd.ExecuteNonQueryAsync();
                }
            }

            ViewData["SuccessMessage"] = "Password reset successful. You can now log in.";
            return RedirectToPage("/login");
        }




    }
}
