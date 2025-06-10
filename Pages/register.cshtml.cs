using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
        private readonly IWebHostEnvironment _env;
        public registerModel(Spendwise_WebApp.DLL.AppDbContext context, IConfiguration configuration, EmailSender emailSender, IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _emailSender = emailSender;
            _env = env;
        }

        [BindProperty]
        public new User User { get; set; } = default!;

        [BindProperty]
        public new AddressData Address { get; set; } = default!;
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
                    { "Address.HouseName", "House name is required." },
                    { "Address.Street", "Street is required." },
                    { "Address.Town", "Town is required." },
                    { "Address.Country", "Country is required." },
                    { "Address.PostCode", "Country is required." },
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

            string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            User.EmailVerificationToken = token;
            User.IsEmailVerified = false;
            User.IsActive = true;
            User.created_on = DateTime.Now;
            _context.Users.Add(User);
            await _context.SaveChangesAsync();

            Address.UserId = User.UserID;
            Address.IsPrimary = true;
            _context.AddressData.Add(Address);
            await _context.SaveChangesAsync();

            AddressData billingAddress = new()
            {
                HouseName = Address.HouseName,
                Street = Address.Street,
                Locality = Address.Locality,
                Town = Address.Town,
                Country = Address.Country,
                County = Address.County,
                PostCode = Address.PostCode,

            };
            billingAddress.UserId = User.UserID;
            billingAddress.IsBilling = true;
            _context.AddressData.Add(billingAddress);
            await _context.SaveChangesAsync();

            string subject = "Welcome to SpendWise Company Formations";

            string pathToFile = Path.Combine(_env.WebRootPath, "EmailTemplate", "Confirm_Account_Registration.html");

            var builder = new MimeKit.BodyBuilder();
            string htmlTemplate;
            using (StreamReader reader = System.IO.File.OpenText(pathToFile))
            {
                htmlTemplate = await reader.ReadToEndAsync();
            }

            var confirmationLink = Url.Page("/ConfirmEmail",
                pageHandler: null,
                values: new { email = User.Email, token = User.EmailVerificationToken },
                protocol: Request.Scheme);

            string emailBody = string.Format(htmlTemplate, subject, User.Email, User.Forename, User.Surname, confirmationLink);

            await _emailSender.SendEmailAsync(User.Email, subject, emailBody);

            return RedirectToPage("/login");
        }


        //public async Task<JsonResult> OnGetCheckPostCode(string PostCode)
        //{
        //    if (string.IsNullOrWhiteSpace(PostCode))
        //    {
        //        return new JsonResult(false);  // Return false if company name is empty
        //    }

        //    string AddressAPikey = _configuration.GetValue<string>("GetAddressApiKey") ?? " ";
        //    var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.getAddress.io/autocomplete/{PostCode}?api-key={AddressAPikey}");

        //    using (var httpClient = new HttpClient())
        //    {
        //        var result = await httpClient.SendAsync(request);
        //        result.EnsureSuccessStatusCode();

        //        string jsonResponse = await result.Content.ReadAsStringAsync();
        //        JObject jsonObject = JObject.Parse(jsonResponse);

        //        return new JsonResult(new { suggestions = jsonObject["suggestions"] });
        //    }
        //}

        public async Task<JsonResult> OnGetCheckPostCodeAsync(string postCode)
        {
            if (string.IsNullOrWhiteSpace(postCode))
            {
                return new JsonResult(new { success = false, message = "Postcode is required.", suggestions = new List<object>() });
            }

            var apiKey = _configuration.GetValue<string>("GetAddressApiKey") ?? " ";
            var encodedPostCode = Uri.EscapeDataString(postCode);
            var apiUrl = $"https://api.getAddress.io/autocomplete/{encodedPostCode}?api-key={apiKey}";

            using var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = false, message = "Failed to fetch address suggestions.", suggestions = new List<object>() });
            }

            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            var suggestionsToken = json["suggestions"];

            // Convert JToken (JArray) to strongly typed list
            var suggestions = suggestionsToken?.ToObject<List<AddressSuggestion>>();

            return new JsonResult(new { success = true, suggestions });
        }


    }
    public class AddressSuggestion
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
