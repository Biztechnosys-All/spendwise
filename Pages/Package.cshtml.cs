using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.Models;

namespace Spendwise_WebApp.Pages
{
    public class PackageModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public PackageModel(ILogger<IndexModel> logger, Spendwise_WebApp.DLL.AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public Package Package { get; set; } = default!;
        public async Task OnGet(string packageName)
        {
            var package = await _context.packages.FirstOrDefaultAsync(m => m.PackageName.ToLower() == packageName.ToLower());
            if (package == null)
            {
                return;
            }
            Package = package;
        }
    }
}
