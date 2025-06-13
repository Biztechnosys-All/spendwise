using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Spendwise_WebApp.DLL;
using Spendwise_WebApp.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.VisualBasic;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Spendwise_WebApp.Pages.BuyPackage
{
    [IgnoreAntiforgeryToken]
    public class CompanyDetailsModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly EmailSender _emailSender;
        private readonly JwtTokenService _jwtTokenService;
        private readonly IWebHostEnvironment _env;

        public CompanyDetailsModel(Spendwise_WebApp.DLL.AppDbContext context, IConfiguration config, EmailSender emailSender, JwtTokenService jwtTokenService, IWebHostEnvironment env)
        {
            _context = context;
            _config = config;
            _emailSender = emailSender;
            _jwtTokenService = jwtTokenService;
            _env = env;
        }

        public string PackageName { get; set; } = string.Empty;
        public Package? SelectedPackage { get; set; }
        [BindProperty]
        public Orders? Order { get; set; }
        [BindProperty]
        public AddressData Address { get; set; }
        public List<AdditionalPackageItem>? additionalPackageItems { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        [BindProperty]
        public User? User { get; set; } = default!;
        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }
        //[TempData]
        //public int? OrderId { get; set; }
        public bool IsUserLoggedIn { get; set; } = false;
        public bool userExist { get; set; } = false!;
        public string PaymentStatusMessage { get; set; } = string.Empty;
        [BindProperty]
        public EmailRequest Input { get; set; }



        public async Task<IActionResult> OnGet(int? id, string? token)
        {
            //var packageName = Request.Cookies["packageName"] ?? "";
            //if (!string.IsNullOrEmpty(packageName))
            //{


            Order = await _context.Orders.Where(x => x.OrderId == id).FirstOrDefaultAsync();
            if(Order.IsPaymentComplete == true)
            {
                return RedirectToPage("/FormationPage/Company-Formation");
            }

            if (TempData.ContainsKey("PaymentStatusMessage"))
            {
                PaymentStatusMessage = TempData["PaymentStatusMessage"]?.ToString();
            }
            //OrderId = Order.OrderId;
            TempData["OrderId"] = Order.OrderId;
            SelectedPackage = await _context.packages.Where(x => x.PackageId == Order.PackageID).FirstOrDefaultAsync();

            var SelectedAddPackageItems = Order != null ? Order.AdditionalPackageItemIds : string.Empty;
            var AddPackageItemIds = SelectedAddPackageItems.Split(',', StringSplitOptions.RemoveEmptyEntries)
                   .Select(int.Parse)
                   .ToList();
            if (!string.IsNullOrEmpty(token) || !string.IsNullOrEmpty(Request.Cookies["AuthToken"]) || !string.IsNullOrEmpty(Request.Cookies["UserEmail"]))
            {
                User = await _context.Users.Where(x => x.Email == Request.Cookies["UserEmail"]).FirstOrDefaultAsync();
                IsUserLoggedIn = true;
                Address = await _context.AddressData.Where(x => x.IsBilling == true && x.UserId == User.UserID).FirstOrDefaultAsync();
            }
            if (AddPackageItemIds == null || AddPackageItemIds.Count() == 0)
            {
                additionalPackageItems = new List<AdditionalPackageItem>();
                return Page();
            }

            additionalPackageItems = await _context.AdditionalPackageItems.Where(x => AddPackageItemIds.Contains(x.AdditionalPackageItemId)).ToListAsync();
            //}

            

            return Page();
        }

        public async Task<JsonResult> OnPostLogin()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                return new JsonResult(new { success = false, message = "Email and password are required." });
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
            var userData = await _context.Users.FirstOrDefaultAsync(x => x.Email == Email);

            if (userData == null)
            {
                ErrorMessage = "User is not registred.";
                return new JsonResult(new { success = false, message = "User is not registred" });
            }

            // Compare hashed passwords
            if (userData.Password != hashedPassword)
            {
                ErrorMessage = "Invalid email or password.";
                return new JsonResult(new { success = false, message = "Invalid email or password." });
            }
            else
            {
                //if (!userData.IsEmailVerified)
                //{
                //    ErrorMessage = "Please verify your email before logging in.";
                //    return new JsonResult(new { success = false, message = "Please verify your email before logging in." });
                //}

                var options = CookieOptionsHelper.GetDefaultOptions();
                Response.Cookies.Append("UserName", userData.Forename + " " + userData.Surname, options);
                Response.Cookies.Append("UserEmail", userData.Email, options);

                var AuthToken = _jwtTokenService.GenerateJwtToken(Email);

                HttpContext.Response.Cookies.Append("AuthToken", AuthToken, options);


                int savedOrderId = (int)TempData["OrderId"];
                string authToken = AuthToken;


                var OrderData = await _context.Orders.Where(x => x.OrderId == savedOrderId).FirstOrDefaultAsync();
                if (OrderData != null)
                {
                    OrderData.OrderBy = userData.UserID;
                    _context.Attach(OrderData).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }

                var savedCompanyId = Request.Cookies["ComanyId"];
                var CompanyData = await _context.CompanyDetails.Where(x => x.CompanyId.ToString() == savedCompanyId).FirstOrDefaultAsync();
                if (CompanyData != null)
                {
                    CompanyData.Createdby = userData.UserID;
                    _context.Attach(CompanyData).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }

                return new JsonResult(new
                {
                    success = true,
                    message = "Login success",
                    orderId = savedOrderId,
                    token = authToken
                });

            }
        }

        public async Task<IActionResult> OnGetForgotPassword(string customerEmail)
        {
            if (string.IsNullOrWhiteSpace(customerEmail))
            {
                return new JsonResult(false);  // Return false if company name is empty
            }

            string resetToken = Guid.NewGuid().ToString(); // Generate unique token
            string connectionString = _config.GetConnectionString("DefaultConnection") ?? "";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "UPDATE Users SET ResetToken = @Token, ResetTokenExpiry = @Expiry WHERE Email = @Email";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", customerEmail);
                    cmd.Parameters.AddWithValue("@Token", resetToken);
                    cmd.Parameters.AddWithValue("@Expiry", DateTime.UtcNow.AddHours(1)); // Token expires in 1 hour
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        ModelState.AddModelError("", "Email not found.");
                        return Page();
                    }
                }
            }

            // Step 2: Send reset email
            string resetLink = Url.Page("/ResetPassword",
                pageHandler: null,
                values: new { email = customerEmail, token = resetToken ?? "" },
                protocol: Request.Scheme) ?? "";

            //await SendResetEmail(customerEmail, resetLink ?? "");
            await _emailSender.SendEmailAsync(customerEmail, "Reset Your Password",
                $"Click the link to reset your password:  <a href='{resetLink}'>Reset Password</a>.");

            ViewData["Message"] = "A password reset link has been sent to your email.";
            return Page();

        }

        public async Task<JsonResult> OnPostCreateNewUser([FromBody] UserPayloadData userPayloadData)
        {
            string tempPassword = string.Empty;
            var userData = await _context.Users.FirstOrDefaultAsync(x => x.Email == userPayloadData.UserData.Email);
            if (userData != null)
            {
                userExist = true;
                return new JsonResult(new { success = false, message = "User Exists!" });
            }
            // Hash the password using MD5
            if (!string.IsNullOrEmpty(userPayloadData.UserData.Password))
            {
                tempPassword = userPayloadData.UserData.Password;
                using (MD5 md5 = MD5.Create())
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(userPayloadData.UserData.Password);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    // Convert the byte array to a hexadecimal string
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("x2"));
                    }
                    userPayloadData.UserData.Password = sb.ToString();
                }
            }

            userPayloadData.UserData.BillingEmail = userPayloadData.UserData.Email;
            userPayloadData.UserData.BillingPhoneNumber = userPayloadData.UserData.PhoneNumber;

            string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            //UserData.EmailVerificationToken = token;
            userPayloadData.UserData.IsEmailVerified = true;
            userPayloadData.UserData.IsActive = true;
            userPayloadData.UserData.created_on = DateTime.Now;
            _context.Users.Add(userPayloadData.UserData);
            await _context.SaveChangesAsync();

            Address = userPayloadData.address;
            Address.UserId = userPayloadData.UserData.UserID;
            Address.IsPrimary = true;
            _context.AddressData.Add(Address);
            await _context.SaveChangesAsync();

            AddressData billingAddress = new()
            {
                HouseName = userPayloadData.address.HouseName,
                Street = userPayloadData.address.Street,
                Locality = userPayloadData.address.Locality,
                Town = userPayloadData.address.Town,
                Country = userPayloadData.address.Country,
                County = userPayloadData.address.County,
                PostCode = userPayloadData.address.PostCode,

            };
            billingAddress.UserId = userPayloadData.UserData.UserID;
            billingAddress.IsBilling = true;
            _context.AddressData.Add(billingAddress);
            await _context.SaveChangesAsync();

            //var confirmationLink = Url.Page("/ConfirmEmail",
            //    pageHandler: null,
            //    values: new { email = UserData.Email, token = UserData.EmailVerificationToken },
            //    protocol: Request.Scheme);

            //await _emailSender.SendEmailAsync(UserData.Email, "Verify Your Email",
            //    $"Click <a href='{confirmationLink}'>here</a> to verify your email.");

            Email = userPayloadData.UserData.Email;
            Password = tempPassword;

            var result = await OnPostLogin() as JsonResult;

            if (result != null)
            {
                dynamic data = result.Value;

                if (data.success == true)
                {
                    // Do something with loginResult, or just return it
                    return result;
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Login failed!" });
                }
            }
            else
            {
                return new JsonResult(new { success = false, message = "Login failed!" });
            }
        }

        public async Task<JsonResult> OnPostUpdateUserBillingAddress([FromBody] AddressData addressData)
        {
            var userEmail = Request.Cookies["UserEmail"] ?? "";
            var userData = await _context.Users.FirstOrDefaultAsync(x => x.Email == userEmail);
            var billingAddData = await _context.AddressData.FirstOrDefaultAsync(x => x.UserId == userData.UserID && x.IsBilling == true);

            if (billingAddData != null)
            {
                _context.AddressData.Remove(billingAddData);
                await _context.SaveChangesAsync();
            }

            addressData.UserId = userData.UserID;
            addressData.IsBilling = true;
            _context.AddressData.Add(addressData);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true, message = "Billing Address is Saved." });
        }

        public async Task<JsonResult> OnGetSetupPayment()
        {
            int OrderRefId = 0;
            double orderAmount = 0;
            int CusOrderId = Convert.ToInt32(Request.Cookies["OrderId"]);
            var OrderData = await _context.Orders.Where(x => x.OrderId == CusOrderId).FirstOrDefaultAsync();
            if (OrderData != null)
            {
                OrderRefId = OrderData.OrderId;
                orderAmount = OrderData.TotalAmount;
            }
            int amountInMinorUnits = (int)Math.Round(orderAmount * 100);

            string url = "https://try.access.worldpay.com/payment_pages";

            // Your API credentials
            string username = "xjg9Ooew6J8gXiYk";
            string password = "9y6XFqPdMxjPVM7cF51rtBnmNHyE3kJMTaLNcuV44IVMSetAhFF09smOgA9n9bq1";
            string base64Credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            string baseUrl = _config.GetValue<string>("BaseURL") ?? " ";
            var jsonBody = $@"
                {{
                  ""transactionReference"": ""{OrderRefId}"",
                  ""merchant"": {{
                    ""entity"": ""PO4079482312""
                  }},
                  ""narrative"": {{
                    ""line1"": ""SpendWise Accountancy""
                  }},
                  ""value"": {{
                    ""currency"": ""GBP"",
                    ""amount"": ""{amountInMinorUnits}""
                  }},
                  ""description"": ""SpendWise Accountancy"",
                  ""resultURLs"": {{
                     ""successURL"": ""{baseUrl}BuyPackage/CompanyDetails?handler=PaymentSuccess"",
                     ""pendingURL"": ""{baseUrl}"",
                     ""failureURL"": ""{baseUrl}BuyPackage/CompanyDetails?handler=PaymentError"",
                     ""errorURL"": ""{baseUrl}BuyPackage/CompanyDetails?handler=PaymentError"",
                     ""cancelURL"": ""{baseUrl}BuyPackage/CompanyDetails?handler=PaymentCancelled"",
                     ""expiryURL"": ""{baseUrl}""
                  }}
                }}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.worldpay.payment_pages-v1.hal+json"));
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/vnd.worldpay.payment_pages-v1.hal+json");

                HttpResponseMessage response = await client.SendAsync(request);
                string result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(result);
                    if (doc.RootElement.TryGetProperty("url", out var urlElement))
                    {
                        string paymentPageUrl = urlElement.GetString();

                        // 🎯 Return the payment page URL to your AJAX call
                        return new JsonResult(new { success = true, paymentPageUrl });
                    }
                    else
                    {
                        return new JsonResult(new { success = false, message = "Payment page URL not found in response." });
                    }
                }
                else
                {
                    return new JsonResult(new { success = false, message = $"Error from Worldpay: {response.StatusCode}", details = result });
                }
            }
        }

        public async Task<IActionResult> OnGetPaymentSuccess()
        {
            int CusOrderId = Convert.ToInt32(Request.Cookies["OrderId"]);
            Order = await _context.Orders.Where(x => x.OrderId == CusOrderId).FirstOrDefaultAsync();

            
            if (Order != null)
            {
                Order.IsPaymentComplete = true;
                await _context.SaveChangesAsync();
            }


            PaymentStatusMessage = "Payment successful! Thank you for your order.";
            return RedirectToPage("/FormationPage/Company-Formation");
        }

        public async Task<IActionResult> OnGetPaymentError(string errorRefNumber, string errors)
        {
            int CusOrderId = Convert.ToInt32(Request.Cookies["OrderId"]);
            Order = await _context.Orders.Where(x => x.OrderId == CusOrderId).FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(errors))
            {
                TempData["PaymentStatusMessage"] = $"Payment error: {errors}";
            }
            else if (!string.IsNullOrEmpty(errorRefNumber))
            {
                TempData["PaymentStatusMessage"] = $"Payment error (Ref: {errorRefNumber}). Please try again.";
            }
            else
            {
                TempData["PaymentStatusMessage"] = "An unknown error occurred during payment. Please try again.";
            }
            return RedirectToPage("/BuyPackage/Index", new { id = CusOrderId });
        }

        public async Task<IActionResult> OnGetPaymentCancelled()
        {
            int CusOrderId = Convert.ToInt32(Request.Cookies["OrderId"]);
            Order = await _context.Orders.Where(x => x.OrderId == CusOrderId).FirstOrDefaultAsync();

            //Console.WriteLine($"Status Code: {response.StatusCode}");
            //Console.WriteLine("Response:");
            //Console.WriteLine(result);
            TempData["PaymentStatusMessage"] = "A Payment Cancelled. Please try again.";
            return RedirectToPage("/BuyPackage/Index", new { id = CusOrderId });
        }

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
        
    public class UserPayloadData
    {
        public User UserData { get; set; }
        public AddressData address { get; set; }
    }
    public class EmailRequest
    {
        public string Email { get; set; }
        public string Otp { get; set; }
    }
}
