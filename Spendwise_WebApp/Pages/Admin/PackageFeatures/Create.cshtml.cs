using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Spendwise_WebApp.DLL;
using Spendwise_WebApp.Models;

namespace Spendwise_WebApp.Pages.Admin.PackageFeatures
{
    public class CreateModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public CreateModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public PackageFeature PackageFeatures { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.PackageFeatures.Add(PackageFeatures);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
