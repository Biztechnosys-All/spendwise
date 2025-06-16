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
    [IgnoreAntiforgeryToken]
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

        public class RegistrationDataRequest
        {
            public User UserData { get; set; }
            public AddressData AddressData { get; set; }
        }

        public async Task<IActionResult> OnPostRegistrationAsync([FromBody] RegistrationDataRequest payload)
        {
            try
            {
                var user = payload.UserData;
                var address = payload.AddressData;

                var userData = await _context.Users.FirstOrDefaultAsync(x => x.Email == user.Email);
                if (userData != null)
                {
                    userExist = true;
                    return Page();
                }
                // Hash the password using MD5
                if (!string.IsNullOrEmpty(user.Password))
                {
                    using (MD5 md5 = MD5.Create())
                    {
                        byte[] inputBytes = Encoding.UTF8.GetBytes(user.Password);
                        byte[] hashBytes = md5.ComputeHash(inputBytes);

                        // Convert the byte array to a hexadecimal string
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < hashBytes.Length; i++)
                        {
                            sb.Append(hashBytes[i].ToString("x2"));
                        }
                        user.Password = sb.ToString();
                    }
                }

                string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

                User registrationData = new()
                {
                    Title = user.Title,
                    Forename = user.Forename,
                    Surname = user.Surname,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    Password = user.Password,
                    BillingEmail = user.Email,
                    BillingPhoneNumber = user.PhoneNumber,
                    EmailVerificationToken = token,
                    IsActive = true,
                    IsEmailVerified = false,
                    created_on = DateTime.Now
                };
                _context.Users.Add(registrationData);
                await _context.SaveChangesAsync();

                AddressData primaryAddress = new()
                {
                    HouseName = address.HouseName,
                    Street = address.Street,
                    Locality = address.Locality,
                    Town = address.Town,
                    Country = address.Country,
                    County = address.County,
                    PostCode = address.PostCode,
                    UserId = registrationData.UserID,
                    IsPrimary = true
                };
                _context.AddressData.Add(primaryAddress);
                await _context.SaveChangesAsync();

                AddressData billingAddress = new()
                {
                    HouseName = address.HouseName,
                    Street = address.Street,
                    Locality = address.Locality,
                    Town = address.Town,
                    Country = address.Country,
                    County = address.County,
                    PostCode = address.PostCode,
                    UserId = registrationData.UserID,
                    IsBilling = true
                };
                _context.AddressData.Add(billingAddress);
                await _context.SaveChangesAsync();

                string subject = "Welcome to SpendWise Company Formations";

                string pathToFile = Path.Combine(_env.WebRootPath, "EmailTemplate", "Confirm_Account_Registration.html");

                string htmlTemplate;
                using (StreamReader reader = System.IO.File.OpenText(pathToFile))
                {
                    htmlTemplate = await reader.ReadToEndAsync();
                }

                //var confirmationLink = Url.Page("/ConfirmEmail",
                //    pageHandler: null,
                //    values: new { email = registrationData.Email, token = registrationData.EmailVerificationToken },
                //    protocol: Request.Scheme);

                string emailBody = string.Format(htmlTemplate, registrationData.Email, registrationData.Forename, registrationData.Surname);

                await _emailSender.SendEmailAsync(registrationData.Email, subject, emailBody);

                return new JsonResult(new { success = true, redirectUrl = Url.Page("/Login") });
            }
            catch(Exception e)
            {
                return Page();
            }
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

        public class EmailRequest
        {
            public string Email { get; set; }
            public string Otp { get; set; }
        }

        [BindProperty]
        public EmailRequest Input { get; set; }

        public async Task<JsonResult> OnPostSendOtp([FromBody] EmailRequest input)
        {
            var otp = new Random().Next(100000, 999999).ToString();

            _context.EmailOtp.Add(new EmailOtp
            {
                Email = input.Email,
                OTP = otp,
                GeneratedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();

            string subject = "Email OTP verification";

            string pathToFile = Path.Combine(_env.WebRootPath, "EmailTemplate", "EmailOTPVerification.html");

            string htmlTemplate;
            using (StreamReader reader = System.IO.File.OpenText(pathToFile))
            {
                htmlTemplate = await reader.ReadToEndAsync();
            }

            string emailBody = string.Format(htmlTemplate, subject, otp);

            await _emailSender.SendEmailAsync(input.Email, subject, emailBody);

            return new JsonResult(new { success = true });
        }

        public async Task<JsonResult> OnPostVerifyOtp([FromBody] EmailRequest input)
        {
            try
            {
                var record = await _context.EmailOtp.Where(x => x.Email == input.Email && x.OTP == input.Otp).OrderByDescending(x => x.GeneratedAt).FirstOrDefaultAsync();

                if (record != null && (DateTime.Now - record.GeneratedAt).TotalMinutes <= 10)
                {
                    return new JsonResult(new { success = true });
                }
                else
                {
                    return new JsonResult(new { success = false });
                }
            }
            catch (Exception e)
            {
                return new JsonResult(new { success = false });
            }
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
