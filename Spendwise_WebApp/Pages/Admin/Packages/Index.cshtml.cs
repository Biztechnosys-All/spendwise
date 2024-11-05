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
    public class IndexModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public IndexModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        public IList<Package> Package { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Package = await _context.packages.ToListAsync();
        }
    }
}
