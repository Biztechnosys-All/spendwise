using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Spendwise_WebApp.Pages
{
    public class accountModel : PageModel
    {
        public string LoginUserName { get; set; } = "";

        public IActionResult OnGet()
        {
            var loggedIn = Request.Cookies["AuthToken"];

            if (string.IsNullOrEmpty(loggedIn))
            {
                return RedirectToPage("/Login");
            }
            LoginUserName = Request.Cookies["UserName"] ?? "";
            return Page();  

        }
    }
}
