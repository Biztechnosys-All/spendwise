using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spendwise_WebApp.Models;

namespace Spendwise_WebApp.Pages.FormationPage
{
    public class CompanyFormationSubTabModel : PageModel
    {
        [BindProperty]
        public Particular Particular { get; set; } = default!;
        public void OnGet()
        {
        }

        public IActionResult OnPostSaveParticular()
        {
            if (!ModelState.IsValid)
            {
                // List of required fields with custom error messages
                var requiredFields = new Dictionary<string, string>
                {
                    { "Particular.CompanyName", "CompanyName is required." },
                    { "Particular.CompanyType", "CompanyType is required." },
                    { "Particular.Jurisdiction", "Jurisdiction is required." },
                    { "Particular.Activities", "Activities is required." },
                    { "Particular.SIC_Code", "SIC_Code is required." },
                };

                // Iterate over required fields and add model errors
                foreach (var field in requiredFields)
                {
                    if (ModelState.ContainsKey(field.Key) && ModelState[field.Key]?.Errors.Count > 0)
                    {
                        ModelState.AddModelError(field.Key, field.Value);
                    }
                }
                return Page();
            }
            return Page();
        }
    }
}
