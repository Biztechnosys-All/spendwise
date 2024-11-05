using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.DLL;
using Spendwise_WebApp.Models;

namespace Spendwise_WebApp.Pages.Admin.PackageFeatures
{
    public class EditModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public EditModel(Spendwise_WebApp.DLL.AppDbContext context)
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

            var packagefeatures =  await _context.PackageFeatures.FirstOrDefaultAsync(m => m.FeatureId == id);
            if (packagefeatures == null)
            {
                return NotFound();
            }
            PackageFeatures = packagefeatures;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(PackageFeatures).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PackageFeaturesExists(PackageFeatures.FeatureId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool PackageFeaturesExists(int id)
        {
            return _context.PackageFeatures.Any(e => e.FeatureId == id);
        }
    }
}
