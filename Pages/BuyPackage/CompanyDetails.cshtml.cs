using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.Models;

namespace Spendwise_WebApp.Pages.BuyPackage
{
    public class CompanyDetailsModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public CompanyDetailsModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        public string PackageName { get; set; } = string.Empty;
        public Package? SelectedPackage { get; set; }
        public Orders? Order { get; set; }
        public List<AdditionalPackageItem>? additionalPackageItems { get; set; }

        public async Task<IActionResult> OnGet(int? id)
        {
            var packageName = Request.Cookies["packageName"] ?? "";
            if (!string.IsNullOrEmpty(packageName))
            {
                Order = await _context.Orders.Where(x => x.OrderId == id).FirstOrDefaultAsync();

                SelectedPackage = await _context.packages.Where(x => x.PackageId == Order.PackageID).FirstOrDefaultAsync();

                var SelectedAddPackageItems = Order != null ? Order.AdditionalPackageItemIds : string.Empty;
                var AddPackageItemIds = SelectedAddPackageItems.Split(',', StringSplitOptions.RemoveEmptyEntries)
                       .Select(int.Parse)
                       .ToList();

                if(AddPackageItemIds == null || AddPackageItemIds.Count() == 0)
                {
                    additionalPackageItems = new List<AdditionalPackageItem>();
                    return Page();
                }

                additionalPackageItems = await _context.AdditionalPackageItems.Where(x => AddPackageItemIds.Contains(x.AdditionalPackageItemId)).ToListAsync();
            }
            return Page();
        }
    }
}
