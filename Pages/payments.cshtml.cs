using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.Models;

namespace Spendwise_WebApp.Pages
{
    public class paymentsModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public paymentsModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        public List<Orders> Order { get; set; } = default!;
        public async Task<IActionResult> OnGet()
        {
            List<Orders> orderData;
            var userEmail = Request.Cookies["UserEmail"];

            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            orderData = await _context.Orders.Where(m => m.OrderBy == userId && m.IsOrderComplete == true && m.InvoicedDate != null).ToListAsync();

            //var orderData = await _context.Orders.Where(m => m.OrderByIP == userId).ToListAsync();
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
    }
}
