using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Spendwise_WebApp.Models;
using System.ComponentModel.Design;

namespace Spendwise_WebApp.Pages
{
    public class CompanySummaryModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;

        public CompanySummaryModel(Spendwise_WebApp.DLL.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Particular Particular { get; set; } = default!;

        [BindProperty]
        public CompanyDetail Company { get; set; } = default!;

        [BindProperty]
        public AddressData RegisterdAddress { get; set; } = default!;

        [BindProperty]
        public List<CompanyOfficer> OfficersList { get; set; } = default!;

        [BindProperty]
        public List<Spendwise_WebApp.Models.Document> UploadedDocuments { get; set; }

        [BindProperty]
        public AddressData Address { get; set; } = default!;
        [BindProperty]
        public List<AddressData> AddressList { get; set; } = default!;

        [BindProperty]
        public string RegistredEmail { get; set; }

        [BindProperty]
        public List<AddressData> PersonAddressList { get; set; } = default!;

        public async Task<IActionResult> OnGet(int id)
        {
            var userEmail = Request.Cookies["UserEmail"];
            Company = _context.CompanyDetails.Where(c => c.CompanyId == id).FirstOrDefault();
            
            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            Particular = await _context.Particulars.FirstOrDefaultAsync(m => m.UserId == userId && m.CompanyId == id);

            var Sic_Code = string.Empty;
            foreach (var item in Particular.SIC_Code.Split(','))
            {
                var SicCode = _context.SicCodes.Where(x => x.SicCode == Convert.ToInt32(item)).FirstOrDefault();
                Sic_Code += SicCode.SicCode + " - " + SicCode.Description + "<br>";

            }
            if (Sic_Code.EndsWith("<br>"))
            {
                Sic_Code = Sic_Code.Substring(0, Sic_Code.Length - 5);
            }
            Particular.SIC_Code = Sic_Code;


            RegisterdAddress = await _context.AddressData.Where(x => x.UserId == userId && x.CompanyId == id).FirstOrDefaultAsync();

            OfficersList = await _context.CompanyOfficers.Where(m => m.UserId == userId && m.CompanyID == id).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnGetDownloadSummaryPDF()
        {
            var userEmail = Request.Cookies["UserEmail"];
            var selectCompanyId = Request.Cookies["ComanyId"];
            var userId = _context.Users.FirstOrDefault(x => x.Email == userEmail)?.UserID;

            if (userId == null || string.IsNullOrEmpty(selectCompanyId))
                return NotFound("Missing user or company ID");

            var ParticularData = await _context.Particulars
                .FirstOrDefaultAsync(m => m.UserId == userId && m.CompanyId.ToString() == selectCompanyId);

            var companyId = _context.CompanyDetails
                .FirstOrDefault(c => c.CompanyId.ToString() == selectCompanyId)?.CompanyId;

            var SicCodesWithDesc = new List<(string Code, string Description)>();

            if (!string.IsNullOrEmpty(ParticularData?.SIC_Code))
            {
                foreach (var item in ParticularData.SIC_Code.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    var code = item.Trim();
                    var desc = _context.SicCodes.FirstOrDefault(x => x.SicCode == Convert.ToInt32(code))?.Description;
                    if (!string.IsNullOrEmpty(desc))
                        SicCodesWithDesc.Add((code, desc));
                }
            }

            UploadedDocuments = await _context.Documents
                .Where(x => x.UserId == userId && x.CompanyId == companyId)
                .ToListAsync();

            RegistredEmail = _context.CompanyDetails
                .Where(x => x.Createdby == userId && x.CompanyId.ToString() == selectCompanyId)
                .FirstOrDefault()?.RegisteredEmail;

            AddressList = _context.AddressData
                .Where(x => x.UserId == userId && x.CompanyId.ToString() == selectCompanyId)
                .ToList();

            var companyOfficerList = _context.CompanyOfficers
                .Where(x => x.UserId == userId && x.CompanyID.ToString() == selectCompanyId)
                .ToList();

            OfficersList = new List<CompanyOfficer>();
            PersonAddressList = new List<AddressData>();
            foreach (var companyofficer in companyOfficerList)
            {
                var officer = await _context.CompanyOfficers
                    .FirstOrDefaultAsync(m => m.UserId == userId && m.OfficerId == companyofficer.OfficerId);
                if (officer != null)
                    OfficersList.Add(officer);

                var personAddList = _context.AddressData
                    .Where(x => x.UserId == userId && x.CompanyId.ToString() == selectCompanyId && x.OfficerId == companyofficer.OfficerId)
                    .ToList();

                PersonAddressList.AddRange(personAddList);
            }

            var RegistredOfficeAddress = AddressList.FirstOrDefault(x => x.IsRegisteredOffice && x.IsCurrent);
            var BusinessOfficeAddress = AddressList.FirstOrDefault(x => x.IsBusiness && x.IsCurrent);

            var pdf = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A4);

                    page.Content().Column(col =>
                    {
                        // Company Name header with background, padding, border
                        col.Item()
                            .Element(e => e
                                .Container()
                                .Background("#d7f5cb")
                                .Padding(5)
                                .Text($"{ParticularData.CompanyName}")
                                .Bold()
                                .FontSize(16)
                                .AlignCenter()
                                .FontColor("#38761d"));

                        // Horizontal line
                        col.Item().LineHorizontal(1).LineColor("#ddd");

                        // Company details table
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(150);
                                columns.RelativeColumn();
                            });

                            static IContainer CellStyle(IContainer container) =>
                                container.PaddingVertical(2);

                            void Row(string label, string? value)
                            {
                                table.Cell().Element(CellStyle).Text(label).Bold();
                                table.Cell().Element(CellStyle).Text(value ?? "N/A");
                            }

                            Row("Company Name", ParticularData.CompanyName);
                            Row("Registered Office", RegistredOfficeAddress != null
                                ? $"{RegistredOfficeAddress.HouseName} {RegistredOfficeAddress.Street}, {RegistredOfficeAddress.Locality}, {RegistredOfficeAddress.County}, {RegistredOfficeAddress.PostCode}, {RegistredOfficeAddress.Country}"
                                : "N/A");
                            Row("Business Office", BusinessOfficeAddress != null
                                ? $"{BusinessOfficeAddress.HouseName} {BusinessOfficeAddress.Street}, {BusinessOfficeAddress.Locality}, {BusinessOfficeAddress.County}, {BusinessOfficeAddress.PostCode}, {BusinessOfficeAddress.Country}"
                                : "N/A");
                            Row("Registered Email", RegistredEmail);
                            Row("Company Type", ParticularData.CompanyType);
                            Row("Jurisdiction", ParticularData.Jurisdiction);
                        });

                        // Section header: Appointments
                        col.Item()
                            .Element(e => e
                                .Container()
                                .PaddingTop(10)
                                .Background("#d7f5cb")
                                .Padding(5)
                                .Text("Appointments")
                                .Bold()
                                .AlignCenter()
                                .FontSize(14)
                                .FontColor("#38761d"));

                        // Officers details
                        foreach (var officer in OfficersList)
                        {
                            var residential = PersonAddressList.FirstOrDefault(x => x.OfficerId == officer.OfficerId && x.IsResidetialAddress);
                            var service = PersonAddressList.FirstOrDefault(x => x.OfficerId == officer.OfficerId && x.IsServiceAddress);

                            if (officer != OfficersList.First())
                            {
                                col.Item().PaddingTop(10); // You can adjust this value (e.g., 5, 15) for more/less space
                            }

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(150);
                                    columns.RelativeColumn();
                                });

                                static IContainer CellStyle(IContainer container) =>
                                    container.PaddingVertical(2);

                                void Row(string label, string? value)
                                {
                                    table.Cell().Element(CellStyle).Text(label).Bold();
                                    table.Cell().Element(CellStyle).Text(value ?? "N/A");
                                }

                                Row("Name", $"{officer.FirstName} {officer.LastName}");
                                Row("Roles", officer.PositionName);
                                Row("DOB", officer.DOB.ToString("dd/MM/yyyy"));
                                Row("Occupation", officer.Occupation);
                                Row("Nationality", officer.Nationality);
                                Row("Residential Address", residential != null
                                    ? $"{residential.HouseName} {residential.Street}, {residential.Locality}, {residential.County}, {residential.PostCode}, {residential.Country}"
                                    : "N/A");
                                Row("Service Address", service != null
                                    ? $"{service.HouseName} {service.Street}, {service.Locality}, {service.County}, {service.PostCode}, {service.Country}"
                                    : "N/A");
                            });
                        }

                        // Documents section, if any
                        if (UploadedDocuments.Any(x => x.DocumentType == "identity" || x.DocumentType == "address"))
                        {
                            col.Item()
                                .Element(e => e
                                    .Container()
                                    .PaddingTop(10)
                                    .Background("#d7f5cb")
                                    .Padding(5)
                                    .Text("Documents")
                                    .Bold()
                                    .AlignCenter()
                                    .FontSize(14)
                                    .FontColor("#38761d"));

                            var identityDocs = UploadedDocuments.Where(d => d.DocumentType == "identity").ToList();
                            if (identityDocs.Any())
                            {
                                col.Item()
                                    .Element(e => e.Container().PaddingTop(5).Text("Proof of Identity").Bold());

                                foreach (var doc in identityDocs)
                                {
                                    col.Item().Text($"{doc.DocumentName}: {doc.FileName}");
                                }
                            }

                            var addressDocs = UploadedDocuments.Where(d => d.DocumentType == "address").ToList();
                            if (addressDocs.Any())
                            {
                                col.Item()
                                    .Element(e => e.Container().PaddingTop(5).Text("Proof of Address").Bold());

                                foreach (var doc in addressDocs)
                                {
                                    col.Item().Text($"{doc.DocumentName}: {doc.FileName}");
                                }
                            }
                        }

                        // SIC Codes section
                        if (SicCodesWithDesc.Any())
                        {
                            col.Item()
                                .Element(e => e
                                    .Container()
                                    .PaddingTop(10)
                                    .Background("#d7f5cb")
                                    .Padding(5)
                                    .Text("SIC Codes")
                                    .Bold()
                                    .AlignCenter()
                                    .FontSize(14)
                                    .FontColor("#38761d"));

                            foreach (var sic in SicCodesWithDesc)
                            {
                                col.Item().Text($"{sic.Code}: {sic.Description}");
                            }
                        }
                    });
                });
            });
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            var pdfBytes = pdf.GeneratePdf();
            return File(pdfBytes, "application/pdf");
        }
    }
}
