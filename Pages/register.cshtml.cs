using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spendwise_WebApp.Models;
using System.Security.Cryptography;
using System.Text;

namespace Spendwise_WebApp.Pages
{
    public class registerModel : PageModel
    {

        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public registerModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public new User User { get; set; } = default!;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Hash the password using MD5
            if (!string.IsNullOrEmpty(User.Password))
            {
                using (MD5 md5 = MD5.Create())
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(User.Password);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    // Convert the byte array to a hexadecimal string
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("x2"));
                    }
                    User.Password = sb.ToString();
                }
            }

            User.IsActive = true;
            User.created_on = DateTime.Now;
            _context.Users.Add(User);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
