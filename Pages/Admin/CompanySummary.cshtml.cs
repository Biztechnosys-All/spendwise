using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Spendwise_WebApp.Models;
using System.Data;

namespace Spendwise_WebApp.Pages.Admin
{
    public class CompanySummaryModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;
        public CompanySummaryModel(IConfiguration configuration, Spendwise_WebApp.DLL.AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
            Summary = new CompanySummaryViewModel
            {
                Documents = new List<DocumentSummary>()
            };
        }

        public CompanySummaryViewModel Summary { get; set; }

        public async Task OnGetAsync(int companyId)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var result = await connection.QueryAsync<CompanySummaryRow>(
                    "sp_GetCompanySummary",
                    new { CompanyId = companyId },
                    commandType: CommandType.StoredProcedure
                );

                var summary = new CompanySummaryViewModel();
                var officers = new Dictionary<int, OfficerSummary>();

                foreach (var row in result)
                {
                    if (summary.CompanyId == 0)
                    {
                        summary.CompanyId = row.CompanyId;
                        summary.CompanyName = row.CompanyName;
                        summary.RegisteredEmail = row.RegisteredEmail;
                        summary.Jurisdiction = row.Jurisdiction;
                        summary.CompanyType = row.CompanyType;
                        summary.SicCode = row.SIC_Code;

                        summary.SicCodesWithDesc = new List<(string Code, string Description)>();
                        if (!string.IsNullOrEmpty(summary.SicCode))
                        {
                            foreach (var item in summary.SicCode.Split(',', StringSplitOptions.RemoveEmptyEntries))
                            {
                                var code = item.Trim();
                                var desc = _context.SicCodes.FirstOrDefault(x => x.SicCode == Convert.ToInt32(code))?.Description;
                                summary.SicCodesWithDesc.Add((code, desc ?? "N/A"));
                            }
                        }

                        summary.RegisteredOfficeAddress = FormatAddress(row.Reg_HouseName, row.Reg_Street, row.Reg_Locality, row.Reg_County, row.Reg_PostCode, row.Reg_Country);
                        summary.BusinessOfficeAddress = FormatAddress(row.Bus_HouseName, row.Bus_Street, row.Bus_Locality, row.Bus_County, row.Bus_PostCode, row.Bus_Country);
                    }

                    // Officer details
                    if (row.OfficerId.HasValue && row.OfficerId.Value != 0)
                    {
                        if (!officers.ContainsKey(row.OfficerId.Value))
                        {
                            var officer = new OfficerSummary
                            {
                                OfficerId = row.OfficerId.Value,
                                FullName = $"{row.FirstName} {row.LastName}",
                                PositionName = row.PositionName,
                                DOB = row.DOB,
                                Occupation = row.Occupation,
                                Nationality = row.Nationality
                            };
                            officers[row.OfficerId.Value] = officer;
                        }

                        var addr = FormatAddress(row.Addr_HouseName, row.Addr_Street, row.Addr_Locality, row.Addr_County, row.Addr_PostCode, row.Addr_Country);

                        if (row.IsResidetialAddress == true)
                            officers[row.OfficerId.Value].ResidentialAddress = addr;

                        if (row.IsServiceAddress == true)
                            officers[row.OfficerId.Value].ServiceAddress = addr;
                    }

                    // Documents
                    if (!string.IsNullOrWhiteSpace(row.DocumentName))
                    {
                        if (!summary.Documents.Any(d => d.DocumentName == row.DocumentName && d.FileName == row.FileName))
                        {
                            summary.Documents.Add(new DocumentSummary
                            {
                                DocumentName = row.DocumentName,
                                FileName = row.FileName
                            });
                        }
                    }
                }

                summary.Officers = officers.Values.ToList();
                Summary = summary;
            }
        }

        private string FormatAddress(params string[] parts)
        {
            return string.Join(", ", parts.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }
}
