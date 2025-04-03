using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spendwise_WebApp.DLL;

namespace Spendwise_WebApp.Pages
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly AppDbContext _dbContext;

        public ConfirmEmailModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string? Message { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string token)
        {
            if (email == null || token == null)
            {
                Message = "Invalid verification request.";
                return Page();
            }

            var user = _dbContext.Users.FirstOrDefault(u => u.Email == email && u.EmailVerificationToken == token);
            if (user == null)
            {
                Message = "Invalid or expired token.";
                return Page();
            }

            user.IsEmailVerified = true;
            user.EmailVerificationToken = ""; // Clear token after verification
            await _dbContext.SaveChangesAsync();

            Message = "Email verified successfully!";
            return Page();
        }

    }
}
