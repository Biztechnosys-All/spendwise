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
        public new AddressData Address { get; set; } = default!;

        [BindProperty]
        public new List<AddressData> AddressList { get; set; } = default!;

        [BindProperty]
        public string? AddressType { get; set; }

        public string Message { get; set; } = string.Empty;

        public async Task<IActionResult> OnGet()
        {
            var loggedIn = Request.Cookies["AuthToken"];

            if (string.IsNullOrEmpty(loggedIn))
            {
                return RedirectToPage("/Login");
            }

            var LoginEmail = Request.Cookies["UserEmail"] ?? "";

            if (!string.IsNullOrEmpty(LoginEmail))
            {
                var userData = await _context.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == LoginEmail.ToLower());
                var addressData = await _context.AddressData.Where(x => x.UserId == userData.UserID).ToListAsync();

                if (userData != null && addressData != null)
                {
                    User = userData;
                    AddressList = addressData;

                }
                Message = TempData["Message"]?.ToString();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Address.AddressId");
            ModelState.Remove("Address.PostCode");
            ModelState.Remove("Address.HouseName");
            ModelState.Remove("Address.Street");
            ModelState.Remove("Address.Locality");
            ModelState.Remove("Address.Town");
            ModelState.Remove("Address.Country");
            ModelState.Remove("Address.County");
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
                    { "Message", "Update Error." }
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

            var existingUser = await _context.Users.FindAsync(User.UserID);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Update only fields that were actually submitted (manually)
            existingUser.Title = User.Title;
            existingUser.Forename = User.Forename;
            existingUser.Surname = User.Surname;
            existingUser.Email = User.Email;
            existingUser.PhoneNumber = User.PhoneNumber;
            existingUser.BillingEmail = string.IsNullOrEmpty(User.BillingEmail) ? User.Email : User.BillingEmail;
            existingUser.BillingPhoneNumber = string.IsNullOrEmpty(User.BillingPhoneNumber) ? User.PhoneNumber : User.BillingPhoneNumber;

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

            TempData["Message"] = "Details updated";
            return RedirectToAction("OnGet");
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserID == id);
        }

        private bool AddressExists(int id)
        {
            return _context.AddressData.Any(e => e.AddressId == id);
        }

        public async Task<IActionResult> OnPostUpdateAddress()
        {
            if (AddressType == "billing")
            {
                Address.HouseName = Address.HouseName;
                Address.Street = Address.Street;
                Address.Town = Address.Town;
                Address.Locality = Address.Locality;
                Address.PostCode = Address.PostCode;
                Address.County = Address.County;
                Address.Country = Address.Country;

                string connectionString = _config.GetConnectionString("DefaultConnection") ?? "";

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "UPDATE AddressData SET HouseName = @HouseName, Street = @Street, Town = @Town, Locality = @Locality, PostCode = @PostCode, County = @County, Country = @Country, IsBilling = @IsBilling WHERE AddressId = @AddressId";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@HouseName", Address.HouseName);
                        cmd.Parameters.AddWithValue("@Street", Address.Street);
                        cmd.Parameters.AddWithValue("@Town", Address.Town);
                        cmd.Parameters.AddWithValue("@Locality", Address.Locality);
                        cmd.Parameters.AddWithValue("@PostCode", Address.PostCode);
                        cmd.Parameters.AddWithValue("@County", Address.County);
                        cmd.Parameters.AddWithValue("@Country", Address.Country);
                        cmd.Parameters.AddWithValue("@IsBilling", true);
                        cmd.Parameters.AddWithValue("@AddressId", Address.AddressId);
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
                Address.HouseName = Address.HouseName;
                Address.Street = Address.Street;
                Address.Town = Address.Town;
                Address.Locality = Address.Locality;
                Address.PostCode = Address.PostCode;
                Address.County = Address.County;
                Address.Country = Address.Country;

                string connectionString = _config.GetConnectionString("DefaultConnection") ?? "";

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "UPDATE AddressData SET HouseName = @HouseName, Street = @Street, Town = @Town, Locality = @Locality, PostCode = @PostCode, County = @County, Country = @Country, IsPrimary = @IsPrimary WHERE AddressId = @AddressId";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@HouseName", Address.HouseName);
                        cmd.Parameters.AddWithValue("@Street", Address.Street);
                        cmd.Parameters.AddWithValue("@Town", Address.Town);
                        cmd.Parameters.AddWithValue("@Locality", Address.Locality);
                        cmd.Parameters.AddWithValue("@PostCode", Address.PostCode);
                        cmd.Parameters.AddWithValue("@County", Address.County);
                        cmd.Parameters.AddWithValue("@Country", Address.Country);
                        cmd.Parameters.AddWithValue("@IsPrimary", true);
                        cmd.Parameters.AddWithValue("@AddressId", Address.AddressId);
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

            TempData["Message"] = "Address is updated";
            return RedirectToAction("OnGet");
        }
    }
}
