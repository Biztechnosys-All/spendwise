using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.DLL;
using Spendwise_WebApp.Models;
using System.Security.Cryptography;
using System.Text;

namespace Spendwise_WebApp.Pages
{
    public class loginModel : PageModel
    {

        private readonly Spendwise_WebApp.DLL.AppDbContext _context;
        private readonly JwtTokenService _jwtTokenService;

        public loginModel(Spendwise_WebApp.DLL.AppDbContext context, JwtTokenService jwtTokenService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
        }

        [BindProperty]
        public new User User { get; set; } = default!;
        public User UserData { get; set; } = default!;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(User.Email) || string.IsNullOrEmpty(User.Password))
            {
                ModelState.AddModelError(string.Empty, "Email and Password are required.");
                return Page();
            }

            string hashedPassword;
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(User.Password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to a hexadecimal string
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                hashedPassword = sb.ToString();
            }
            var userData = await _context.Users.FirstOrDefaultAsync(x => x.Email == User.Email);

            if (userData == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return Page();
            }

            // Compare hashed passwords
            if (userData.Password != hashedPassword)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return Page();
            }
            else
            {
                var options = CookieOptionsHelper.GetDefaultOptions();
                Response.Cookies.Append("UserName", userData.Forename +" "+ userData.Surname, options);

                HttpContext.Response.Cookies.Append("AuthToken", _jwtTokenService.GenerateJwtToken(User.Email), options);

                return RedirectToPage("./account");

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

    }
}
