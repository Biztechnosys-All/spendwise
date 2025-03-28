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

        public new User User { get; set; }
        public void OnGet()
        {
            var LoginEmail = Request.Cookies["UserEmail"] ?? "";

            if (!string.IsNullOrEmpty(LoginEmail))
            {
                var userData = _context.Users.FirstOrDefaultAsync(x => x.Email == LoginEmail);

                if (userData != null)
                {
                    //User = userData;

                }
            }
        }
    }
}
