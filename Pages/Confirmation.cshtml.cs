using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Spendwise_WebApp.Pages
{
    [IgnoreAntiforgeryToken]
    public class ConfirmationModel : PageModel
    {
        [BindProperty]
        public int OrderId { get; set; }
        public IActionResult OnGet()
        {
            OrderId = Convert.ToInt32(Request.Cookies["OrderId"]);
            return Page();
        }
    }
}
