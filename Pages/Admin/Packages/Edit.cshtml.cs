using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NuGet.Protocol.Plugins;
using Spendwise_WebApp.DLL;
using Spendwise_WebApp.Models;
using static System.Net.Mime.MediaTypeNames;

namespace Spendwise_WebApp.Pages.Admin.Packages
{
    public class EditModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public EditModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Package Package { get; set; } = default!;
        public List<SelectListItem> PackageFeatureList { get; set; } = default!;
        public string[] SelectedFeaturesID = [];

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            PackageFeatureList = _context.PackageFeatures.Select(p => new SelectListItem
            {
                Text = p.Feature,
                Value = p.FeatureId.ToString(),
            }).ToList();

            var package = await _context.packages.FirstOrDefaultAsync(m => m.PackageId == id);
            if (package == null)
            {
                return NotFound();
            }
            Package = package;

            SelectedFeaturesID = package.PackageFeatures.Split(',').ToArray();
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
            var PackagesFeatures = Request.Form["Package.PackageFeatures"];
            Package.PackageFeatures = PackagesFeatures;

            _context.Attach(Package).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PackageExists(Package.PackageId))
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

        private bool PackageExists(int id)
        {
            return _context.packages.Any(e => e.PackageId == id);
        }
    }
}
