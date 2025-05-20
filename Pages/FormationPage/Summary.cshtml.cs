using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.Models;

namespace Spendwise_WebApp.Pages.FormationPage
{
    public class SummaryModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;
        private readonly IConfiguration _config;

        public SummaryModel(Spendwise_WebApp.DLL.AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        [BindProperty]
        public Particular Particular { get; set; } = default!;
        [BindProperty]
        public AddressData Address { get; set; } = default!;
        [BindProperty]
        public List<AddressData> AddressList { get; set; } = default!;
        [BindProperty]
        public string RegistredEmail { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var userEmail = Request.Cookies["UserEmail"];
            var selectCompanyId = Request.Cookies["ComanyId"];

            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            Particular = await _context.Particulars.FirstOrDefaultAsync(m => m.UserId == userId && m.CompanyId.ToString() == selectCompanyId);

            var Sic_Code_desc = string.Empty;
            foreach (var item in Particular.SIC_Code.Split(','))
            {
                var SicDesc = _context.SicCodes.Where(x => x.SicCode == Convert.ToInt32(item)).FirstOrDefault().Description;
                Sic_Code_desc += SicDesc + ",<br>";

            }
            if (Sic_Code_desc.EndsWith(",<br>"))
            {
                Sic_Code_desc = Sic_Code_desc.Substring(0, Sic_Code_desc.Length - 5);
            }

            Particular.SIC_Code = Sic_Code_desc;

            #region Registered Office Address
            RegistredEmail = _context.CompanyDetails.Where(x=> x.Createdby == userId && x.CompanyId.ToString() == selectCompanyId).FirstOrDefault().RegisteredEmail;
            var OfficeAddress = _context.AddressData.Where(x => x.UserId == userId && x.CompanyId.ToString() == selectCompanyId).ToList();
            AddressList = OfficeAddress;


            #endregion

            return Page();
        }
    }
}
