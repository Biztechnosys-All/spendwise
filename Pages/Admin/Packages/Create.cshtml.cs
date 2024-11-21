using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Spendwise_WebApp.DLL;
using Spendwise_WebApp.Models;

namespace Spendwise_WebApp.Pages.Admin.Packages
{
    public class CreateModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;


        public CreateModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        public List<SelectListItem> PackageFeatureList { get; set; } = default!;

        public IActionResult OnGet()
        {
            PackageFeatureList = _context.PackageFeatures.Select(p => new SelectListItem
            {
                Text = p.Feature,
                Value = p.FeatureId.ToString()
            }).ToList();
            return Page();
        }

        [BindProperty]
        public Package Package { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
           var PackagesFeatures =  Request.Form["Package.PackageFeatures"];
            Package.PackageFeatures = PackagesFeatures;
            Package.created_on = DateTime.Now;
            _context.packages.Add(Package);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
