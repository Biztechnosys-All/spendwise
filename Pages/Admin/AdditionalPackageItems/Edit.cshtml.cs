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

namespace Spendwise_WebApp.Pages.Admin.AdditionalPackageItems
{
    public class EditModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public EditModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public AdditionalPackageItem additionalPackageItem { get; set; } = default!;
        public List<SelectListItem> PackageList { get; set; } = default!;
        public string SelectedPackageID = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            PackageList = _context.packages.Select(p => new SelectListItem
            {
                Text = p.PackageName,
                Value = p.PackageName.ToString()
            }).ToList();

            var AdditionalPackageItem = await _context.AdditionalPackageItems.FirstOrDefaultAsync(m => m.AdditionalPackageItemId == id);
            if (AdditionalPackageItem == null)
            {
                return NotFound();
            }
            additionalPackageItem = AdditionalPackageItem;

            SelectedPackageID = AdditionalPackageItem.PackageName.ToString();
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

            _context.Attach(additionalPackageItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PackageExists(additionalPackageItem.AdditionalPackageItemId))
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
