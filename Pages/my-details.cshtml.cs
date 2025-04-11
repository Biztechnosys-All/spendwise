using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Spendwise_WebApp.Models;
using System.Security.Cryptography;
using System.Text;

namespace Spendwise_WebApp.Pages
{
    public class my_detailsModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;
        private readonly IConfiguration _config;

        public my_detailsModel(Spendwise_WebApp.DLL.AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [BindProperty]
        public new User User { get; set; } = default!;

        [BindProperty]
        public string? AddressType { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var LoginEmail = Request.Cookies["UserEmail"] ?? "";

            if (!string.IsNullOrEmpty(LoginEmail))
            {
                var userData = await _context.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == LoginEmail.ToLower());

                if (userData != null)
                {
                    User = userData;

                }
            }
            return Page();
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
                    { "User.Locality", "Locality is required." },
                    { "User.Town", "Town is required." },
                    { "User.Country", "Country is required." },
                    { "User.County", "County is required." },
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

            User.BillingEmail = User.BillingEmail != null ? User.BillingEmail : User.Email;
            User.BillingPhoneNumber = User.BillingPhoneNumber != null ? User.BillingPhoneNumber : User.PhoneNumber;
            User.BillingHouseName = User.BillingHouseName != null ? User.BillingHouseName : User.HouseName;
            User.BillingStreet = User.BillingStreet != null ? User.BillingStreet : User.Street;
            User.BillingTown = User.BillingTown != null ? User.BillingTown : User.Town;
            User.BillingLocality = User.BillingLocality != null ? User.BillingLocality : User.Locality;
            User.BillingPostCode = User.BillingPostCode != null ? User.BillingPostCode : User.PostCode;
            User.BillingCounty = User.BillingCounty != null ? User.BillingCounty : User.County;
            User.BillingCountry = User.BillingCountry != null ? User.BillingCountry : User.Country;

            _context.Attach(User).State = EntityState.Modified;

            try
            {
                if (string.IsNullOrEmpty(User.Password))
                {
                    var userData = await _context.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == User.Email.ToLower());
                    if (userData != null)
                    {
                        User.Password = userData.Password;
                    }
                }
                else
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
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(User.UserID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Page();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserID == id);
        }

        public async Task<IActionResult> OnPostUpdateAddress()
        {
            if (AddressType == "billing")
            {
                User.BillingHouseName = User.BillingHouseName;
                User.BillingStreet = User.BillingStreet;
                User.BillingTown = User.BillingTown;
                User.BillingLocality = User.BillingLocality;
                User.BillingPostCode = User.BillingPostCode;
                User.BillingCounty = User.BillingCounty;
                User.BillingCountry = User.BillingCountry;
                
                string connectionString = _config.GetConnectionString("DefaultConnection") ?? "";

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "UPDATE Users SET BillingHouseName = @BillingHouseName,BillingStreet = @BillingStreet, BillingTown = @BillingTown, BillingLocality = @BillingLocality, BillingPostCode = @BillingPostCode, BillingCounty = @BillingCounty, BillingCountry = @BillingCountry WHERE UserID = @UserID";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BillingHouseName", User.BillingHouseName);
                        cmd.Parameters.AddWithValue("@BillingStreet", User.BillingStreet);
                        cmd.Parameters.AddWithValue("@BillingTown", User.BillingTown);
                        cmd.Parameters.AddWithValue("@BillingLocality", User.BillingLocality);
                        cmd.Parameters.AddWithValue("@BillingPostCode", User.BillingPostCode);
                        cmd.Parameters.AddWithValue("@BillingCounty", User.BillingCounty);
                        cmd.Parameters.AddWithValue("@BillingCountry", User.BillingCountry);
                        cmd.Parameters.AddWithValue("@UserID", User.UserID);
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected == 0)
                        {
                            ModelState.AddModelError("", "Data not found.");
                            return Page();
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                User.HouseName = User.HouseName;
                User.Street = User.Street;
                User.Town = User.Town;
                User.Locality = User.Locality;
                User.PostCode = User.PostCode;
                User.County = User.County;
                User.Country = User.Country;

                string connectionString = _config.GetConnectionString("DefaultConnection") ?? "";

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "UPDATE Users SET HouseName = @HouseName, Street = @Street, Town = @Town, Locality = @Locality, PostCode = @PostCode, County = @County, Country = @Country WHERE UserID = @UserID";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@HouseName", User.HouseName);
                        cmd.Parameters.AddWithValue("@Street", User.Street);
                        cmd.Parameters.AddWithValue("@Town", User.Town);
                        cmd.Parameters.AddWithValue("@Locality", User.Locality);
                        cmd.Parameters.AddWithValue("@PostCode", User.PostCode);
                        cmd.Parameters.AddWithValue("@County", User.County);
                        cmd.Parameters.AddWithValue("@Country", User.Country);
                        cmd.Parameters.AddWithValue("@UserID", User.UserID);
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected == 0)
                        {
                            ModelState.AddModelError("", "Data not found.");
                            return Page();
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./my-details");
        }
    }
}
