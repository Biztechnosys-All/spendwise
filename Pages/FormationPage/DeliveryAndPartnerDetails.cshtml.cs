using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.DLL;
using Spendwise_WebApp.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Spendwise_WebApp.Pages.FormationPage
{
    [IgnoreAntiforgeryToken]
    public class DeliveryAndPartnerDetailsModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public DeliveryAndPartnerDetailsModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Package? SelectedPackage { get; set; }

        [BindProperty]
        public string? CompanyName { get; set; }

        [BindProperty]
        public Orders? Order { get; set; }

        [BindProperty]
        public List<AdditionalPackageItem>? AdditionalPackageItems { get; set; }

        [BindProperty]
        public CompanyDetail Company { get; set; } = default!;

        public async Task<IActionResult> OnGet()
        {
            var orderId = Convert.ToInt32(Request.Cookies["OrderId"]);
            Order = await _context.Orders.Where(x => x.OrderId == orderId).FirstOrDefaultAsync();
            SelectedPackage = await _context.packages.Where(x => x.PackageId == Order.PackageID).FirstOrDefaultAsync();

            var SelectedAddPackageItems = Order != null ? Order.AdditionalPackageItemIds : string.Empty;
            var AddPackageItemIds = SelectedAddPackageItems.Split(',', StringSplitOptions.RemoveEmptyEntries)
                   .Select(int.Parse)
                   .ToList();

            CompanyName = Request.Cookies["companyName"];

            if (AddPackageItemIds == null || AddPackageItemIds.Count() == 0)
            {
                AdditionalPackageItems = new List<AdditionalPackageItem>();
                return Page();
            }

            AdditionalPackageItems = await _context.AdditionalPackageItems.Where(x => AddPackageItemIds.Contains(x.AdditionalPackageItemId)).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostSubmitCompanyToReview()
        {
            CompanyName = Request.Cookies["companyName"];
            var companyId = Request.Cookies["ComanyId"]; // typo: should probably be "CompanyId"
            Company = await _context.CompanyDetails.FirstOrDefaultAsync(m => m.CompanyId.ToString() == companyId);

            if (Company != null)
            {
                Company.CompanyStatus = "InReview";
                await _context.SaveChangesAsync(); // Use await with async SaveChangesAsync()
            }

            return new JsonResult(new { success = true });
        }
    }
}
