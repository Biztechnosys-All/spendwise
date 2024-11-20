using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.Models;

namespace Spendwise_WebApp.Pages.BuyPackage
{
    public class SelectPackageModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public IList<Package> Package { get; set; } = default!;
        public IList<PackageFeature> packageFeatureList { get; set; } = default!;

        public SelectPackageModel(ILogger<IndexModel> logger, IConfiguration configuration, Spendwise_WebApp.DLL.AppDbContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        public async Task OnGet()
        {
            Package = await _context.packages.ToListAsync();
            packageFeatureList = await _context.PackageFeatures.ToListAsync();
        }
    }
}
