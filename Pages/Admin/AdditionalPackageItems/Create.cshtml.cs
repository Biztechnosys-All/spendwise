using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Spendwise_WebApp.DLL;
using Spendwise_WebApp.Models;

namespace Spendwise_WebApp.Pages.Admin.AdditionalPackageItems
{
    public class CreateModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;


        public CreateModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        public List<SelectListItem> PackageList { get; set; } = default!;

        public IActionResult OnGet()
        {
            PackageList = _context.packages.Select(p => new SelectListItem
            {
                Text = p.PackageName,
                Value = p.PackageName.ToString()
            }).ToList();
            return Page();
        }

        [BindProperty]
        public AdditionalPackageItem additionalPackageItem { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                PackageList = _context.packages.Select(p => new SelectListItem
                {
                    Text = p.PackageName,
                    Value = p.PackageName.ToString()
                }).ToList();
                return Page();
            }

            _context.AdditionalPackageItems.Add(additionalPackageItem);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
