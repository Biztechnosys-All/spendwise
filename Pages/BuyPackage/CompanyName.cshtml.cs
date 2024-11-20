using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Spendwise_WebApp.Pages.BuyPackage
{
    public class CompanyNameModel : PageModel
    {
        public void OnGet()
        {
        }
        public IActionResult OnPost()
        {
            var companyname = Request.Form["name"];
            if (companyname != "")
            {
                return new OkResult();
            }
            return Page();
        }
    }
}
