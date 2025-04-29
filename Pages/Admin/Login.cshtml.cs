using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.DLL;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Spendwise_WebApp.Pages.Admin
{
    public class LoginModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;
        private readonly JwtTokenService _jwtTokenService;


        public LoginModel(Spendwise_WebApp.DLL.AppDbContext context, JwtTokenService jwtTokenService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
        }

        [BindProperty]
        public required string Username { get; set; } = string.Empty;
        [BindProperty]
        public required string Password { get; set; } = string.Empty;
        [BindProperty]
        public string ErrorMessage { get; set; } = string.Empty;


        public IActionResult OnGet()
        {
            var loggedIn = Request.Cookies["IsAdminLoggedIn"];

            if (loggedIn != "true")
            {
                return Page();
            }
            else
            {
                return RedirectToPage("/Admin/Dashboard");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Email and Password are required.";
                return Page();
            }

            string hashedPassword;
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(Password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to a hexadecimal string
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                hashedPassword = sb.ToString();
            }
            var userData = await _context.Users.FirstOrDefaultAsync(x => x.Email == Username && x.IsActive == true && x.IsAdmin == true);

            if (userData == null)
            {
                ErrorMessage = "User is not registred.";
                return Page();
            }

            // Compare hashed passwords
            if (userData.Password != hashedPassword)
            {
                ErrorMessage = "Invalid email or password.";
                return Page();
            }
            else
            {
                //var options = CookieOptionsHelper.GetDefaultOptions();
                //Response.Cookies.Append("UserName", userData.Forename + " " + userData.Surname, options);
                //Response.Cookies.Append("UserEmail", userData.Email, options);

                //Response.Cookies.Append("AuthToken", _jwtTokenService.GenerateJwtToken(Username), options);


                Response.Cookies.Append("IsAdminLoggedIn", "true", new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.Now.AddHours(1)
                });

                return RedirectToPage("./Dashboard");

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

            return RedirectToPage("/Admin/Login");
        }

    }
}
