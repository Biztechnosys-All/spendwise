using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.DLL;
using Spendwise_WebApp.Models;

namespace Spendwise_WebApp.Pages
{
    public class companiesModel : PageModel
    {
        private readonly AppDbContext _context;

        public companiesModel(AppDbContext context)
        {
            _context = context;
        }

        public List<CompanyDetail> Companies { get; set; } = default!;

        public async Task<IActionResult> OnGet()
        {
            //List<CompanyDetail> companiesData;
            var userEmail = Request.Cookies["UserEmail"];

            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            var companiesData = await _context.CompanyDetails.Where(m => m.Createdby == userId).ToListAsync();

            if (companiesData == null)
            {
                return NotFound();
            }
            else
            {
                Companies = companiesData;
            }
            return Page();
        }
    }
}
