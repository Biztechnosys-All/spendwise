using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Spendwise_WebApp.DLL;
using Spendwise_WebApp.Models;
using System.Security.Cryptography;
using System.Text;

namespace Spendwise_WebApp.Pages.BuyPackage
{
    [IgnoreAntiforgeryToken]
    public class CompanyDetailsModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly EmailSender _emailSender;
        private readonly JwtTokenService _jwtTokenService;

        public CompanyDetailsModel(Spendwise_WebApp.DLL.AppDbContext context, IConfiguration config, EmailSender emailSender, JwtTokenService jwtTokenService)
        {
            _context = context;
            _config = config;
            _emailSender = emailSender;
            _jwtTokenService = jwtTokenService;
        }

        public string PackageName { get; set; } = string.Empty;
        public Package? SelectedPackage { get; set; }
        [BindProperty]
        public Orders? Order { get; set; }
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


        public async Task<IActionResult> OnGet(int? id, string? token)
        {
            //var packageName = Request.Cookies["packageName"] ?? "";
            //if (!string.IsNullOrEmpty(packageName))
            //{


            Order = await _context.Orders.Where(x => x.OrderId == id).FirstOrDefaultAsync();
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
                if (!userData.IsEmailVerified)
                {
                    ErrorMessage = "Please verify your email before logging in.";
                    return new JsonResult(new { success = false, message = "Please verify your email before logging in." });
                }

                var options = CookieOptionsHelper.GetDefaultOptions();
                Response.Cookies.Append("UserName", userData.Forename + " " + userData.Surname, options);
                Response.Cookies.Append("UserEmail", userData.Email, options);

                var AuthToken = _jwtTokenService.GenerateJwtToken(Email);

                HttpContext.Response.Cookies.Append("AuthToken", AuthToken, options);


                int savedOrderId = (int)TempData["OrderId"];
                string authToken = AuthToken;


               var OrderData = await _context.Orders.Where(x => x.OrderId == savedOrderId).FirstOrDefaultAsync();
                if(OrderData != null)
                {
                    OrderData.OrderBy = userData.UserID;
                    _context.Attach(OrderData).State = EntityState.Modified;
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

        public async Task<JsonResult> OnPostCreateNewUser([FromBody] User UserData)
        {
            string tempPassword = string.Empty;
            var userData = await _context.Users.FirstOrDefaultAsync(x => x.Email == UserData.Email);
            if (userData != null)
            {
                userExist = true;
                return new JsonResult(new { success = false, message = "User Exists!" });
            }
            // Hash the password using MD5
            if (!string.IsNullOrEmpty(UserData.Password))
            {
                tempPassword = UserData.Password;
                using (MD5 md5 = MD5.Create())
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(UserData.Password);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    // Convert the byte array to a hexadecimal string
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("x2"));
                    }
                    UserData.Password = sb.ToString();
                }
            }

            UserData.BillingEmail = UserData.Email;
            UserData.BillingPhoneNumber = UserData.PhoneNumber;
            UserData.BillingHouseName = UserData.HouseName;
            UserData.BillingStreet = UserData.Street;
            UserData.BillingTown = UserData.Town;
            UserData.BillingLocality = UserData.Locality;
            UserData.BillingPostCode = UserData.PostCode;
            UserData.BillingCounty = UserData.County;
            UserData.BillingCountry = UserData.Country;

            string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            //UserData.EmailVerificationToken = token;
            UserData.IsEmailVerified = true;
            UserData.IsActive = true;
            UserData.created_on = DateTime.Now;
            _context.Users.Add(UserData);
            await _context.SaveChangesAsync();

            //var confirmationLink = Url.Page("/ConfirmEmail",
            //    pageHandler: null,
            //    values: new { email = UserData.Email, token = UserData.EmailVerificationToken },
            //    protocol: Request.Scheme);

            //await _emailSender.SendEmailAsync(UserData.Email, "Verify Your Email",
            //    $"Click <a href='{confirmationLink}'>here</a> to verify your email.");

            Email = UserData.Email;
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

    }
}
