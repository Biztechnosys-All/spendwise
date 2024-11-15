using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Spendwise_WebApp.Pages.BuyPackage
{
    public class IndexModel : PageModel
    {
        public string PackageName { get; set; } = string.Empty;

        public void OnGet(string packageName)
        {
            if(!string.IsNullOrEmpty(packageName))
            {
                PackageName = packageName;
            }
        }
    }
}
