using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spendwise_WebApp.Models;
using System.ComponentModel.Design;
using System.Net.Http;
using System.Text;

namespace Spendwise_WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public CompanyList? companyList { get; set; }
        public IList<Package> Package { get; set; } = default!;
        [BindProperty]
        public string CompanyName { get; set; } = "";


        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration, Spendwise_WebApp.DLL.AppDbContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        public async Task OnGet()
        {

            // Create a cookie with a value and expiration time
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(7), // Cookie expires in 7 days
                HttpOnly = true, // Prevent JavaScript access for security
                Secure = true, // Use HTTPS
                IsEssential = true // Required for GDPR compliance
            };

            Response.Cookies.Append("UserPreference", "DarkMode", cookieOptions);


            var PackageFeatureList = await _context.PackageFeatures.ToListAsync();
            Package = await _context.packages.ToListAsync();

            foreach (var item in Package)
            {
                var selectedFeatures = string.Empty;
                var last = item.PackageFeatures.Split(',').Last();
                foreach (var FeatureId in item.PackageFeatures.Split(','))
                {
                    if (FeatureId.Equals(last))
                    {
                        selectedFeatures += PackageFeatureList.Where(x => x.FeatureId.ToString() == FeatureId).Select(y => y.Feature).FirstOrDefault().ToString();
                    }
                    else
                    {
                        selectedFeatures += PackageFeatureList.Where(x => x.FeatureId.ToString() == FeatureId).Select(y => y.Feature).FirstOrDefault().ToString() + ", ";
                    }

                }
                item.PackageFeatures = selectedFeatures;
            }

        }

        public async Task<IActionResult> OnPost()
        {
            //var CompanyName = Request.Form["CompanyName"];

            //string APikey = _configuration.GetValue<string>("CompaniesHouseApiKey") ?? " ";
            //var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.company-information.service.gov.uk/company/{CompanyID}");
            //var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.company-information.service.gov.uk/alphabetical-search/companies?q={CompanyName}");
            //var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.company-information.service.gov.uk/search/companies?q={CompanyName}&restrictions=active-companies legally-equivalent-company-name");

            //request.Headers.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes(APikey))}");
            //using (var httpClient = new HttpClient())
            //{
            //    var result = await httpClient.SendAsync(request);
            //    result.EnsureSuccessStatusCode();

            //    string jsonResponse = await result.Content.ReadAsStringAsync();

            //}
            Package = await _context.packages.ToListAsync();
            return Page();
        }


        public async Task<IActionResult> OnGetCheckCompanyAvailability(string CompanyName)
        {
            if (string.IsNullOrWhiteSpace(CompanyName))
            {
                return new JsonResult(false);  // Return false if company name is empty
            }


            string APikey = _configuration.GetValue<string>("CompaniesHouseApiKey") ?? " ";
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.company-information.service.gov.uk/search/companies?q={CompanyName}&restrictions=active-companies legally-equivalent-company-name");

            request.Headers.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes(APikey))}");
            using (var httpClient = new HttpClient())
            {
                var result = await httpClient.SendAsync(request);
                result.EnsureSuccessStatusCode();

                string jsonResponse = await result.Content.ReadAsStringAsync();

                JObject jsonObject = JObject.Parse(jsonResponse);
                JArray itemsArray = (JArray)jsonObject["items"];

                // If items array is null or empty, the company name is available
                bool isAvailable = itemsArray == null || itemsArray.Count == 0;

                return new JsonResult(isAvailable);
            }
        }

    }
}
