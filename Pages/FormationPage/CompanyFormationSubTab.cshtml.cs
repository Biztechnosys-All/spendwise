using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.Models;

namespace Spendwise_WebApp.Pages.FormationPage
{
    [IgnoreAntiforgeryToken]
    public class CompanyFormationSubTabModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;
        public CompanyFormationSubTabModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Particular Particular { get; set; } = default!;
        public User User { get; set; } = default!;
        public List<SelectListItem> SicCodeCategoryList { get; set; } = default!;
        public IActionResult OnGet()
        {
            SicCodeCategoryList = _context.SicCodeCategory.Select(p => new SelectListItem
            {
                Text = p.Category,
                Value = p.Section.ToString()
            }).ToList();
            return Page();
        }
        public async Task<JsonResult> OnPostSaveParticularData([FromBody] Particular particularData)
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
                    { "Particular.SIC_Code", "SIC_Code is required." }
                };

                // Iterate over required fields and add model errors
                foreach (var field in requiredFields)
                {
                    if (ModelState.ContainsKey(field.Key) && ModelState[field.Key]?.Errors.Count > 0)
                    {
                        ModelState.AddModelError(field.Key, field.Value);
                    }
                }
                return new JsonResult(new { success = false });
            }

            _context.Particular.Add(particularData);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        public JsonResult OnGetGetSicCodeByCategory(string section)
        {
            try
            {
                if (!string.IsNullOrEmpty(section))
                {
                    var sicCodeList = _context.SicCodes
                        .Where(x => x.Section.ToLower() == section.ToLower())
                        .ToList();

                    return new JsonResult(new { success = true, data = sicCodeList });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Section is required." });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "An error occurred.", error = ex.Message });
            }

        }
    }
}
