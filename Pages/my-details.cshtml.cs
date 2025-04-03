using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.Models;

namespace Spendwise_WebApp.Pages
{
    public class my_detailsModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public my_detailsModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public new User User { get; set; } = default!;

        public async Task<IActionResult> OnGet()
        {
            var LoginEmail = Request.Cookies["UserEmail"] ?? "";

            if (!string.IsNullOrEmpty(LoginEmail))
            {
                var userData = await _context.Users.FirstOrDefaultAsync(x =>  x.Email.ToLower() == LoginEmail.ToLower());

                if (userData != null)
                {
                    User = userData;

                }
            }
            return Page();
        }
    }
}
