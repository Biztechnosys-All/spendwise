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
        public async Task<IActionResult> OnGet(string packageName)
        {
            var PackageFeatureList = await _context.PackageFeatures.ToListAsync();
            var package = await _context.packages.FirstOrDefaultAsync(m => m.PackageName.ToLower() == packageName.ToLower());
            if (package == null)
            {
                return RedirectToPage("/Error");
            }

            var selectedFeatures = string.Empty;
            var featureIds = package.PackageFeatures.Split(',');
            var last = featureIds.Last();

            foreach (var featureId in featureIds)
            {
                var featureName = PackageFeatureList
                    .Where(x => x.FeatureId.ToString() == featureId)
                    .Select(y => y.Feature)
                    .FirstOrDefault();

                if (featureId.Equals(last))
                    selectedFeatures += featureName;
                else
                    selectedFeatures += featureName + ", ";
            }
            package.PackageFeatures = selectedFeatures;
            Package = package;
            return Page();
        }
    }
}
