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

        public IList<Package> Package { get; set; } = default!;
        [BindProperty]
        public string CompanyName { get; set; } = "";
        public bool isuUserLogin { get; set; } = false;


        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration, Spendwise_WebApp.DLL.AppDbContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        public async Task OnGet()
        {
            var userName = Request.Cookies["UserName"];
            if (!string.IsNullOrEmpty(userName))
            {
                isuUserLogin = true;
            }
            else
            {
                isuUserLogin = false;
            }

            var PackageFeatureList = await _context.PackageFeatures.ToListAsync();
            Package = await _context.packages.Where(x=>x.IsLimitedCompanyPkg == true).ToListAsync();

            //foreach (var item in Package)
            //{
            //    var selectedFeatures = string.Empty;
            //    var last = item.PackageFeatures.Split(',').Last();
            //    foreach (var FeatureId in item.PackageFeatures.Split(','))
            //    {
            //        if (FeatureId.Equals(last))
            //        {
            //            selectedFeatures += PackageFeatureList.Where(x => x.FeatureId.ToString() == FeatureId).Select(y => y.Feature).FirstOrDefault().ToString();
            //        }
            //        else
            //        {
            //            selectedFeatures += PackageFeatureList.Where(x => x.FeatureId.ToString() == FeatureId).Select(y => y.Feature).FirstOrDefault().ToString() + ", ";
            //        }

            //    }
            //    item.PackageFeatures = selectedFeatures;
            //}

            foreach (var package in Package)
            {
                var featureIds = package.PackageFeatures.Split(',');
                var selectedFeaturesList = new List<string>();

                foreach (var featureId in featureIds)
                {
                    var featureItem = PackageFeatureList.FirstOrDefault(x => x.FeatureId.ToString() == featureId.Trim());
                    if (featureItem != null)
                    {
                        // Combine Feature and PackageInfo as needed
                        var combined = $"{featureItem.Feature} ({featureItem.FeatureInfo})";
                        selectedFeaturesList.Add(combined);
                    }
                }

                // Join with comma + space
                package.PackageFeatures = string.Join(", ", selectedFeaturesList);
            }
        }

        public async Task<IActionResult> OnPost()
        {
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
