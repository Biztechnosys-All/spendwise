using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.DLL;
using Spendwise_WebApp.Models;

namespace Spendwise_WebApp.Pages.Admin.Packages
{
    public class DetailsModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public DetailsModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        public Package Package { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var package = await _context.packages.FirstOrDefaultAsync(m => m.PackageId == id);
            if (package == null)
            {
                return NotFound();
            }
            else
            {
                Package = package;
            }
            return Page();
        }
    }
}
