using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.DLL;
using Spendwise_WebApp.Models;

namespace Spendwise_WebApp.Pages.Admin.PackageFeatures
{
    public class DeleteModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public DeleteModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public PackageFeature PackageFeatures { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var packagefeatures = await _context.PackageFeatures.FirstOrDefaultAsync(m => m.FeatureId == id);

            if (packagefeatures == null)
            {
                return NotFound();
            }
            else
            {
                PackageFeatures = packagefeatures;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var packagefeatures = await _context.PackageFeatures.FindAsync(id);
            if (packagefeatures != null)
            {
                PackageFeatures = packagefeatures;
                _context.PackageFeatures.Remove(PackageFeatures);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
