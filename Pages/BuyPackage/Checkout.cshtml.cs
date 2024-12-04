using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Spendwise_WebApp.Models;

namespace Spendwise_WebApp.Pages.BuyPackage
{
    public class CheckoutModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public CheckoutModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        public Package SelectedPackage { get; set; }
        public List<AdditionalPackageItem> additionalPackageItems { get; set; }

        public async Task OnGet(string packageName)
        {
            if (!string.IsNullOrEmpty(packageName))
            {
                SelectedPackage = await _context.packages.Where(x => x.PackageName.ToLower() == packageName.ToLower()).FirstOrDefaultAsync();

                additionalPackageItems = await _context.AdditionalPackageItems.Where(x => x.PackageName == SelectedPackage.PackageName).ToListAsync();

            }
        }

        public AdditionalPackageItem GetAdditinalPackageItem(int ItemId)
        {
            AdditionalPackageItem additionalItem = new AdditionalPackageItem();
            if (ItemId > 0)
            {
               additionalItem = _context.AdditionalPackageItems.Where(x => x.AdditionalPackageItemId == ItemId).FirstOrDefault();
            }
            return additionalItem;
        }
    }
}
