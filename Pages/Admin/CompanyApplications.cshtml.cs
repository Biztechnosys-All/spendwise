using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Spendwise_WebApp.Pages.Admin
{
    [IgnoreAntiforgeryToken]
    public class CompanyApplicationsModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public CompanyApplicationsModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public List<SubmittedCompany>? SubmittedCompanyList { get; set; }

        public class RequestModel
        {
            public string status { get; set; }
            public int CompanyId { get; set; }
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
            var CompanyName = Request.Cookies["companyName"];
            var companyId = request.CompanyId; // typo: should probably be "CompanyId"
            var Company = await _context.CompanyDetails.FirstOrDefaultAsync(m => m.CompanyId == companyId);

            if (Company != null)
            {
                Company.CompanyStatus = request.status;
                await _context.SaveChangesAsync(); // Use await with async SaveChangesAsync()
            }

            return new JsonResult(new { success = true });
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
