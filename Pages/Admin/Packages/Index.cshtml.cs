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
    public class IndexModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public IndexModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        public IList<Package> Package { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            var loggedIn = Request.Cookies["IsAdminLoggedIn"];

            if (loggedIn != "true")
            {
                return RedirectToPage("/Admin/Login");
            }
            Package = await _context.packages.ToListAsync();
            var PackageFeatureList = await _context.PackageFeatures.ToListAsync();
             
            foreach (var item in Package)
            {
                var selectedFeatures = string.Empty;
                var last = item.PackageFeatures.Split(',').Last();
                foreach (var FeatureId in item.PackageFeatures.Split(','))
                {
                    if(FeatureId.Equals(last))
                    {
                        selectedFeatures += PackageFeatureList.Where(x => x.FeatureId.ToString() == FeatureId).Select(y => y.Feature).FirstOrDefault().ToString();
                    }
                    else
                    {
                        selectedFeatures += PackageFeatureList.Where(x => x.FeatureId.ToString() == FeatureId).Select(y => y.Feature).FirstOrDefault().ToString() + ", ";
                    }
                    
                }
                item.PackageFeatures = selectedFeatures;
            }
            return Page();
        }
    }
}
