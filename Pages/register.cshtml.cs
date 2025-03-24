using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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
        public bool userExist { get; set; } = false!;
        public bool isAgreeTerms  { get; set; } = false!;


        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // List of required fields with custom error messages
                var requiredFields = new Dictionary<string, string>
                {
                    { "User.Title", "Title is required." },
                    { "User.Forename", "Forename is required." },
                    { "User.Surname", "Surname is required." },
                    { "User.PhoneNumber", "Phone number is required." },
                    { "User.Email", "Email is required." },
                    { "User.Password", "Password is required." },
                    { "User.PostCode", "Postcode is required." },
                    { "User.HouseName", "House name is required." },
                    { "User.Street", "Street is required." },
                    { "User.Town", "Town is required." },
                    { "User.Country", "Country is required." }
                };

                // Iterate over required fields and add model errors
                foreach (var field in requiredFields)
                {
                    if (ModelState.ContainsKey(field.Key) && ModelState[field.Key]?.Errors.Count > 0)
                    {
                        ModelState.AddModelError(field.Key, field.Value);
                    }
                }


                return Page();
            }
            if (!isAgreeTerms)
            {
                ModelState.AddModelError("isAgreeTerms", "Please agree to the Terms and Conditions & Privacy Policy to complete the registration process!");
                return Page();
            }

            var userData = await _context.Users.FirstOrDefaultAsync(x => x.Email == User.Email);
            if (userData != null)
            {
                userExist = true;
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
