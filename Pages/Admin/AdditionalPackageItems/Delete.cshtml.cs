using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.DLL;
using Spendwise_WebApp.Models;

namespace Spendwise_WebApp.Pages.Admin.AdditionalPackageItems
{
    public class DeleteModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public DeleteModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public AdditionalPackageItem additionalPackageItem { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var packageItem = await _context.AdditionalPackageItems.FirstOrDefaultAsync(m => m.AdditionalPackageItemId == id);

            if (packageItem == null)
            {
                return NotFound();
            }
            else
            {
                additionalPackageItem = packageItem;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var packageItem = await _context.AdditionalPackageItems.FindAsync(id);
            if (packageItem != null)
            {
                additionalPackageItem = packageItem;
                _context.AdditionalPackageItems.Remove(additionalPackageItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
