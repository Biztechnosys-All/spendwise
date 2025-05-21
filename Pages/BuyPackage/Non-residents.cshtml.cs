using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.Models;

namespace Spendwise_WebApp.Pages.BuyPackage
{
    public class Non_residentsModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public Non_residentsModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Package Package { get; set; } = default!;
        public async Task<IActionResult> OnGet()
        {
            Package = await _context.packages.FirstOrDefaultAsync(m => m.PackageName.ToLower() == "Non-Residents");
            return Page();
        }
    }
}
