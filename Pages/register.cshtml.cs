using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Spendwise_WebApp.Models;
using System.Security.Cryptography;
using System.Text;

namespace Spendwise_WebApp.Pages
{
    public class registerModel : PageModel
    {

        private readonly Spendwise_WebApp.DLL.AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailSender _emailSender;
        public registerModel(Spendwise_WebApp.DLL.AppDbContext context, IConfiguration configuration, EmailSender emailSender)
        {
            _context = context;
            _configuration = configuration;
            _emailSender = emailSender;
        }

        [BindProperty]
        public new User User { get; set; } = default!;
        public bool userExist { get; set; } = false!;
        [BindProperty]
        public required bool isAgreeTerms { get; set; }
        public required bool AgreeTermsError { get; set; } = false!;


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
                    { "User.Country", "Country is required." },
                    { "isAgreeTerms", "Please agree to the Terms and Conditions & Privacy Policy to complete the registration process!" }
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
                AgreeTermsError = true;
                //ModelState.AddModelError("isAgreeTerms", "");
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

            User.BillingEmail = User.Email;
            User.BillingPhoneNumber = User.PhoneNumber;
            User.BillingHouseName = User.HouseName;
            User.BillingStreet = User.Street;
            User.BillingTown = User.Town;
            User.BillingLocality = User.Locality;
            User.BillingPostCode = User.PostCode;
            User.BillingCounty = User.County;
            User.BillingCountry = User.Country;

            string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            User.EmailVerificationToken = token;
            User.IsEmailVerified = false;
            User.IsActive = true;
            User.created_on = DateTime.Now;
            _context.Users.Add(User);
            await _context.SaveChangesAsync();

            var confirmationLink = Url.Page("/ConfirmEmail",
                pageHandler: null,
                values: new { email = User.Email, token = User.EmailVerificationToken },
                protocol: Request.Scheme);

            string EnailBody = $@"
                <p>Hi there,</p>
                
                <p>Welcome to <strong>SpendWise</strong>! We’re excited to have you on board.</p>
                
                <p>To get started, please verify your email address by clicking the button below:</p>
                
                <p style='text-align: center;'>
                  <a href='{confirmationLink}' 
                     style='display: inline-block; padding: 12px 24px; background-color: #4CAF50; color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: bold;'>
                     Verify Email
                  </a>
                </p>
                
                <p>Thanks,<br>The SpendWise Team</p>
                ";

            await _emailSender.SendEmailAsync(User.Email, "Welcome to SpendWise – Please Verify Your Email Address",
                EnailBody);

            return RedirectToPage("/login");
        }


        public async Task<IActionResult> OnGetCheckPostCode(string PostCode)
        {
            if (string.IsNullOrWhiteSpace(PostCode))
            {
                return new JsonResult(false);  // Return false if company name is empty
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.postcodes.io/postcodes/{PostCode}");

            using (var httpClient = new HttpClient())
            {
                var result = await httpClient.SendAsync(request);
                result.EnsureSuccessStatusCode();

                string jsonResponse = await result.Content.ReadAsStringAsync();

                JObject jsonObject = JObject.Parse(jsonResponse);


                return new JsonResult("");
            }
        }

    }
}
