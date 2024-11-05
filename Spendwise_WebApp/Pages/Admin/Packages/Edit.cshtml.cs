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
                Selected = false
            }).ToList();

            var package =  await _context.packages.FirstOrDefaultAsync(m => m.PackageId == id);
            if (package == null)
            {
                return NotFound();
            }
            //var selectedFeatures = string.Empty;
            //foreach (var item in package.PackageFeatures.Split(','))
            //{
            //    selectedFeatures += PackageFeatureList.Where(x => x.Value == item).Select(y => y.Text).FirstOrDefault().ToString() + ",";
            //}
            //package.PackageFeatures = selectedFeatures;
            Package = package;

           

            //.Where(x => x.Value.Contains(package.PackageFeatures))

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
