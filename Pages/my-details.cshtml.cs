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
                    { "Address.PostCode", "Postcode is required." },
                    { "Address.HouseName", "House name is required." },
                    { "Address.Street", "Street is required." },
                    { "Address.Locality", "Locality is required." },
                    { "Address.Town", "Town is required." },
                    { "Address.Country", "Country is required." },
                    { "Address.County", "County is required." },
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
            //User.BillingHouseName = User.BillingHouseName != null ? User.BillingHouseName : User.HouseName;
            //User.BillingStreet = User.BillingStreet != null ? User.BillingStreet : User.Street;
            //User.BillingTown = User.BillingTown != null ? User.BillingTown : User.Town;
            //User.BillingLocality = User.BillingLocality != null ? User.BillingLocality : User.Locality;
            //User.BillingPostCode = User.BillingPostCode != null ? User.BillingPostCode : User.PostCode;
            //User.BillingCounty = User.BillingCounty != null ? User.BillingCounty : User.County;
            //User.BillingCountry = User.BillingCountry != null ? User.BillingCountry : User.Country;

            _context.Attach(User).State = EntityState.Modified;
            _context.Attach(Address).State = EntityState.Modified;

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

            return RedirectToPage("./my-details");
        }
    }
}
