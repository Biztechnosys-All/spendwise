using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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


        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration, Spendwise_WebApp.DLL.AppDbContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        public async Task OnGet()
        {
            Package = await _context.packages.ToListAsync();
        }

        public async Task<IActionResult> OnPost()
        {
            var CompanyName = Request.Form["CompanyName"];
            string APikey = _configuration.GetValue<string>("CompaniesHouseApiKey") ?? " ";
            //var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.company-information.service.gov.uk/company/{CompanyID}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.company-information.service.gov.uk/alphabetical-search/companies?q={CompanyName}");

            request.Headers.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes(APikey))}");
            using (var httpClient = new HttpClient())
            {
                var result = await httpClient.SendAsync(request);
                result.EnsureSuccessStatusCode();

                string jsonResponse = await result.Content.ReadAsStringAsync();

                var data = JsonConvert.DeserializeObject<CompanyList>(jsonResponse);
                if (data != null)
                {
                    companyList = data;
                }
                Package = await _context.packages.ToListAsync();
                return Page();

            }
        }

    }
}
