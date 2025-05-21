using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Spendwise_WebApp.Models;
using Microsoft.AspNetCore.Authentication;
using System.Net;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using Spendwise_WebApp.DLL;
using Microsoft.Extensions.Options;

namespace Spendwise_WebApp.Pages.BuyPackage
{
    public class CheckoutModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public CheckoutModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        public Package SelectedPackage { get; set; }
        public User User { get; set; }
        public List<AdditionalPackageItem> additionalPackageItems { get; set; }

        public async Task OnGet(string packageName)
        {
            if (!string.IsNullOrEmpty(packageName))
            {
                SelectedPackage = await _context.packages.Where(x => x.PackageName.ToLower() == packageName.ToLower()).FirstOrDefaultAsync();

                additionalPackageItems = await _context.AdditionalPackageItems.Where(x => x.PackageName == SelectedPackage.PackageName).ToListAsync();

            }
        }

        public AdditionalPackageItem GetAdditinalPackageItem(int ItemId)
        {
            AdditionalPackageItem additionalItem = new AdditionalPackageItem();
            if (ItemId > 0)
            {
                additionalItem = _context.AdditionalPackageItems.Where(x => x.AdditionalPackageItemId == ItemId).FirstOrDefault();
            }
            return additionalItem;
        }

        [ValidateAntiForgeryToken]
        public IActionResult OnPostAddCheckOutDetails()
        {
            var userEmail = Request.Cookies["UserEmail"];

            var userIp = userEmail == null ? HttpContext.Connection.RemoteIpAddress.ToString() : "";

            string? netAmount = Request.Cookies["NetAmount"];

            User = userEmail != null ? _context.Users.Where(x => x.Email == userEmail).FirstOrDefault() : null;

            string SelectedItemIdsCsv = string.Empty;

            var rawCookie = Request.Cookies["selectedPackageItems"];
            if (!string.IsNullOrEmpty(rawCookie))
            {
                // 1. URL-decode the cookie string
                var decodedJson = HttpUtility.UrlDecode(rawCookie);

                // 2. Deserialize the JSON array of integers
                List<int> itemIds = JsonSerializer.Deserialize<List<int>>(decodedJson);

                // 3. Convert to CSV
                SelectedItemIdsCsv = string.Join(",", itemIds);
            }

            var companyName = Request.Cookies["companyName"];
            var selectedPackage = _context.packages.Where(x => x.PackageName.ToLower() == (string.IsNullOrEmpty(Request.Cookies["SelectedPackage"]) ? Request.Cookies["packageName"] : Request.Cookies["SelectedPackage"]).ToLower()).FirstOrDefault();


            var companyDetails = new CompanyDetail
            {
                CompanyName = Request.Cookies["companyName"],
                CompanyStatus = "InComplete",
                CompanyType = selectedPackage.PackageName,
                Createdby = User != null ? User.UserID : 0,
                Createdon = DateTime.Now,
            };

            _context.CompanyDetails.Add(companyDetails);
            _context.SaveChanges();

            var companyId = companyDetails.CompanyId;
            Response.Cookies.Append("ComanyId", companyId.ToString());

            var order = new Orders
            {
                OrderBy = User != null ? User.UserID : 0,
                OrderByIP = userIp,
                OrderDate = DateTime.Now,
                PackageID = selectedPackage.PackageId,
                PackageName = selectedPackage.PackageName,
                CompanyName = Request.Cookies["companyName"],
                CompanyId = companyId,
                NetAmount = Convert.ToDouble(Request.Cookies["NetAmount"].Replace("£", "")),
                VatAmount = Convert.ToDouble(Request.Cookies["VatAmount"].Replace("£", "")),
                TotalAmount = Convert.ToDouble(Request.Cookies["TotalAmount"].Replace("£", "")),
                AmountDue = Convert.ToDouble(Request.Cookies["TotalAmount"].Replace("£", "")),
                AdditionalPackageItemIds = SelectedItemIdsCsv,
                IsOrderComplete = false,
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            int orderId = order.OrderId;
            return new JsonResult(new
            {
                success = true,
                orderId = orderId
            });
        }
    }
}
