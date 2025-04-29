using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace Spendwise_WebApp.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        public IActionResult OnGet()
        {
            var loggedIn = Request.Cookies["IsAdminLoggedIn"];

            if (loggedIn != "true")
            {
                return RedirectToPage("/Admin/Login");
            }
            return Page();
        }
    }
}
