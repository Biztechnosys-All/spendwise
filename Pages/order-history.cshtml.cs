using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.Models;
using System.Text.Json;

namespace Spendwise_WebApp.Pages
{
    [IgnoreAntiforgeryToken]
    public class order_historyModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public order_historyModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        public List<Orders> Order { get; set; } = default!;
        public Orders? ItemDetail { get; set; }

        public async Task<IActionResult> OnGet(int? id)
        {
            var loggedIn = Request.Cookies["AuthToken"];

            if (string.IsNullOrEmpty(loggedIn))
            {
                return RedirectToPage("/Login");
            }

            List<Orders> orderData;
            var userEmail = Request.Cookies["UserEmail"];

            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            orderData = await _context.Orders.Where(m => m.OrderBy == userId).ToListAsync();

            if (orderData == null)
            {
                return NotFound();
            }
            else
            {
                Order = orderData;
            }
            return Page();
        }

        public void OnGetRedirectCompanyDetailsPage(int? id)
        {
            ItemDetail = _context.Orders.FirstOrDefault(x => x.OrderId == id);
        }

        public class RequestModel
        {
            public int Id { get; set; }
        }

        public async Task<IActionResult> OnPostDeleteAsync([FromBody] RequestModel request)
        {
            var item = await _context.Orders.FindAsync(request.Id);
            if(item == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(item);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }
    }
}
