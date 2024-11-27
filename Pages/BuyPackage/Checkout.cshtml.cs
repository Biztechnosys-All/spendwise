using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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

        public async Task OnGet(string packageName)
        {
            if (!string.IsNullOrEmpty(packageName))
            {
                SelectedPackage = await _context.packages.Where(x => x.PackageName.ToLower() == packageName.ToLower()).FirstOrDefaultAsync();


            }
        }
    }
}
