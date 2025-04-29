using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Spendwise_WebApp.Pages.FormationPage
{
    public class Company_FormationModel : PageModel
    {
        public IActionResult OnGet()
        {
            var loggedIn = Request.Cookies["AuthToken"];

            if (string.IsNullOrEmpty(loggedIn))
            {
                return RedirectToPage("/Login");
            }
            return Page();
        }
    }
}
