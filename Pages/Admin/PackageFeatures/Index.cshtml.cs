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
    public class IndexModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public IndexModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        public IList<PackageFeature> PackageFeatures { get;set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            var loggedIn = Request.Cookies["IsAdminLoggedIn"];

            if (loggedIn != "true")
            {
                return RedirectToPage("/Admin/Login");
            }
            PackageFeatures = await _context.PackageFeatures.ToListAsync();
            return Page();
        }
    }
}
