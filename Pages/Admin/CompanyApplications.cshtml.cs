using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.Models;
using System.Text.Json;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.AspNetCore.Http;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Net.Mail;
using Spendwise_WebApp.DLL;

namespace Spendwise_WebApp.Pages.Admin
{
    [IgnoreAntiforgeryToken]
    public class CompanyApplicationsModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IWebHostEnvironment _env;
        private readonly EmailSender _emailSender;
        private readonly InvoicePdfGenerator _InvoicepdfGenerator;

        public CompanyApplicationsModel(Spendwise_WebApp.DLL.AppDbContext context, IHttpClientFactory clientFactory, IWebHostEnvironment env, EmailSender emailSender, InvoicePdfGenerator invoicepdfGenerator)
        {
            _context = context;
            _clientFactory = clientFactory;
            _env = env;
            _emailSender = emailSender;
            _InvoicepdfGenerator = invoicepdfGenerator;
        }

        [BindProperty]
        public List<SubmittedCompany>? SubmittedCompanyList { get; set; }

        public class RequestModel
        {
            public string status { get; set; }
            public int CompanyId { get; set; }
            public int OrderNo { get; set; }
        }

        public async Task<IActionResult> OnGet()
        {
            var loggedIn = Request.Cookies["IsAdminLoggedIn"];

            if (loggedIn != "true")
            {
                return RedirectToPage("/Admin/Login");
            }
            var CompanyList = await _context.CompanyDetails.Where(x => x.CompanyStatus == "InReview").ToListAsync();
            SubmittedCompanyList = new List<SubmittedCompany>();
            foreach (var item in CompanyList)
            {
                var companyData = new SubmittedCompany();
                companyData.CompanyName = item.CompanyName;

                SubmittedCompanyList.Add(companyData);
            }

            SubmittedCompanyList = await (from c in _context.CompanyDetails
                                          join u in _context.Users on c.Createdby equals u.UserID
                                          join o in _context.Orders on c.CompanyId equals o.CompanyId
                                          where c.CompanyStatus != "InComplete"
                                          select new SubmittedCompany
                                          {
                                              CompanyId = c.CompanyId,
                                              CompanyName = c.CompanyName,
                                              UserEmail = u.Email,
                                              OrderNo = o.OrderId,
                                              CompnayStatus = c.CompanyStatus
                                          }).ToListAsync();
            return Page();
        }


        public async Task<IActionResult> OnPostUpdateCompanyStatus([FromBody] RequestModel request)
        {
            string ResponseMsg = string.Empty;
            var CompanyName = Request.Cookies["companyName"];
            var companyId = request.CompanyId;
            var OrderId = request.OrderNo;

            var Company = await _context.CompanyDetails.FirstOrDefaultAsync(m => m.CompanyId == companyId);
            var OrderData = await _context.Orders.FirstOrDefaultAsync(x => x.OrderId == OrderId);
            var UserDetails = await _context.Users.FirstOrDefaultAsync(x => x.UserID == OrderData.OrderBy);
            var CompanyParticulars = await _context.Particulars.FirstOrDefaultAsync(x => x.CompanyId == companyId);


            if (Company != null)
            {
                Company.CompanyStatus = request.status;
                Company.ApprovedDate = DateTime.Now;
                await _context.SaveChangesAsync(); // Use await with async SaveChangesAsync()
            }
            if (request.status == "Approved")
            {
                string? CreatedCompanyNumber = string.Empty;
                string? CompanyAuthNumber = string.Empty;
                var client = _clientFactory.CreateClient("CompanyHouseClient");
                List<Attachment> AttchmentList = new List<Attachment>();
                var RequestBody = new CreateTestCompanyRequestBody
                {
                    CompanyName = Company.CompanyName,
                    CompanyType = Company.CompanyType?.ToLower(),
                    Jurisdiction = "england-wales",//CompanyParticulars.Jurisdiction
                    CompanyStatus = "active",
                    CompanyStatusDetail = "active",
                    AccountsDueStatus = "due-soon",
                    HasSuperSecurePscs = false,
                    NumberOfAppointments = 1,
                    OfficerRoles = new List<string> { "director" },
                    SubType = "none"
                };
                var json = JsonSerializer.Serialize(RequestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("test-data/company", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<CreateTestCompanyResponse>(responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    CreatedCompanyNumber = result?.CompanyNumber;
                    CompanyAuthNumber = result?.CompanyAuthCode;
                    ResponseMsg = $"Name: {Company.CompanyName}, Number: {result?.CompanyNumber}, Auth: {result?.CompanyAuthCode}";

                    Company.CompanyNumber = result?.CompanyNumber;
                    Company.CompanyAuthCode = result?.CompanyAuthCode;
                    await _context.SaveChangesAsync(); // Use await with async SaveChangesAsync()

                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error: {response.StatusCode}");
                    Console.WriteLine($"Error: {response.StatusCode}");
                    ResponseMsg = $"Error: {response.StatusCode}";
                    return new JsonResult(new { success = false, Message = ResponseMsg });
                }

                string subject = "Your company application order is processed";
                string pathToFile = Path.Combine(_env.WebRootPath, "EmailTemplate", "OrderFulfillment.html");
                string htmlTemplate;
                using (StreamReader reader = System.IO.File.OpenText(pathToFile))
                {
                    htmlTemplate = await reader.ReadToEndAsync();
                }
                string emailBody = string.Format(htmlTemplate, UserDetails?.Forename, UserDetails?.Surname, OrderData?.PackageName, CreatedCompanyNumber, CompanyAuthNumber);
         
                var pdfBytes = _InvoicepdfGenerator.GenerateInvoicePdf(orderId: OrderData?.OrderId, userEmail: UserDetails?.Email);

                Attachment attachment = new Attachment(new MemoryStream(pdfBytes), "Invoice_"+ OrderData?.OrderId + ".pdf", "application/pdf");
                AttchmentList.Add(attachment);

                await _emailSender.SendEmailAsync(UserDetails?.Email, subject, emailBody, AttchmentList);
            }


            return new JsonResult(new { success = true,Message = ResponseMsg });
        }
    }

    public class SubmittedCompany
    {
        public string CompanyName { get; set; }
        public string UserEmail { get; set; }
        public int CompanyId { get; set; }
        public int OrderNo { get; set; }
        public string CompnayStatus { get; set; }
    }
}
