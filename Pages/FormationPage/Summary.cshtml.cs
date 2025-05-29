using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NReco.PdfGenerator;
using Spendwise_WebApp.Models;
using System.ComponentModel.Design;
using System.Text;

namespace Spendwise_WebApp.Pages.FormationPage
{
    public class SummaryModel : PageModel
    {
        private readonly Spendwise_WebApp.DLL.AppDbContext _context;
        private readonly IConfiguration _config;

        public SummaryModel(Spendwise_WebApp.DLL.AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        [BindProperty]
        public Particular Particular { get; set; } = default!;
        [BindProperty]
        public AddressData Address { get; set; } = default!;
        [BindProperty]
        public List<AddressData> AddressList { get; set; } = default!;

        [BindProperty]
        public string RegistredEmail { get; set; }
        [BindProperty]
        public List<CompanyOfficer> OfficersList { get; set; } = default!;
        [BindProperty]
        public List<AddressData> PersonAddressList { get; set; } = default!;

        [BindProperty]
        public List<Document> UploadedDocuments { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var userEmail = Request.Cookies["UserEmail"];
            var selectCompanyId = Request.Cookies["ComanyId"];

            var userId = _context.Users.Where(x => x.Email == userEmail).FirstOrDefault().UserID;
            Particular = await _context.Particulars.FirstOrDefaultAsync(m => m.UserId == userId && m.CompanyId.ToString() == selectCompanyId);
            var companyId = _context.CompanyDetails.Where(c => c.CompanyId.ToString() == selectCompanyId.ToString()).FirstOrDefault().CompanyId;

            var Sic_Code_desc = string.Empty;
            foreach (var item in Particular.SIC_Code.Split(','))
            {
                var SicDesc = _context.SicCodes.Where(x => x.SicCode == Convert.ToInt32(item)).FirstOrDefault().Description;
                Sic_Code_desc += SicDesc + ",<br>";

            }
            if (Sic_Code_desc.EndsWith(",<br>"))
            {
                Sic_Code_desc = Sic_Code_desc.Substring(0, Sic_Code_desc.Length - 5);
            }
            UploadedDocuments = await _context.Documents.Where(x => x.UserId == userId && x.CompanyId == companyId).ToListAsync();
            Particular.SIC_Code = Sic_Code_desc;

            #region Registered Office Address
            RegistredEmail = _context.CompanyDetails.Where(x => x.Createdby == userId && x.CompanyId.ToString() == selectCompanyId).FirstOrDefault()?.RegisteredEmail;
            var OfficeAddress = _context.AddressData.Where(x => x.UserId == userId && x.CompanyId.ToString() == selectCompanyId).ToList();
            AddressList = OfficeAddress;
            #endregion

            #region Appoitment Data
            var companyOfficerList = _context.CompanyOfficers.Where(x => x.UserId == userId && x.CompanyID.ToString() == selectCompanyId).ToList();
            foreach (var companyofficer in companyOfficerList)
            {
                OfficersList = new List<CompanyOfficer>();
                var officer = await _context.CompanyOfficers.Where(m => m.UserId == userId && m.OfficerId == companyofficer.OfficerId).FirstOrDefaultAsync();
                OfficersList.Add(officer);

                var personAddList = _context.AddressData.Where(x => x.UserId == userId && x.CompanyId.ToString() == selectCompanyId && x.OfficerId == companyofficer.OfficerId).ToList();
                PersonAddressList = personAddList;
            }
            #endregion

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

            // List of pairs: SIC Code + Description
            var SicCodesWithDesc = new List<(string Code, string Description)>();

            if (!string.IsNullOrEmpty(ParticularData?.SIC_Code))
            {
                foreach (var item in ParticularData.SIC_Code.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    var code = item.Trim();
                    var desc = _context.SicCodes.FirstOrDefault(x => x.SicCode == Convert.ToInt32(code))?.Description;

                    if (!string.IsNullOrEmpty(desc))
                    {
                        SicCodesWithDesc.Add((code, desc));
                    }
                }
            }

            UploadedDocuments = await _context.Documents
                .Where(x => x.UserId == userId && x.CompanyId == companyId)
                .ToListAsync();

            #region Registered Office Address
            RegistredEmail = _context.CompanyDetails.Where(x => x.Createdby == userId && x.CompanyId.ToString() == selectCompanyId).FirstOrDefault()?.RegisteredEmail;
            var OfficeAddress = _context.AddressData.Where(x => x.UserId == userId && x.CompanyId.ToString() == selectCompanyId).ToList();
            AddressList = OfficeAddress;
            #endregion

            #region Appoitment Data
            var companyOfficerList = _context.CompanyOfficers.Where(x => x.UserId == userId && x.CompanyID.ToString() == selectCompanyId).ToList();
            foreach (var companyofficer in companyOfficerList)
            {
                OfficersList = new List<CompanyOfficer>();
                var officer = await _context.CompanyOfficers.Where(m => m.UserId == userId && m.OfficerId == companyofficer.OfficerId).FirstOrDefaultAsync();
                OfficersList.Add(officer);

                var personAddList = _context.AddressData.Where(x => x.UserId == userId && x.CompanyId.ToString() == selectCompanyId && x.OfficerId == companyofficer.OfficerId).ToList();
                PersonAddressList = personAddList;
            }
            #endregion

            var RegistredOfficeAddress = AddressList.Where(x => x.IsRegisteredOffice == true && x.IsCurrent == true).FirstOrDefault();
            var BusinessOfficeAddress = AddressList.Where(x => x.IsBusiness == true && x.IsCurrent == true).FirstOrDefault();

            var htmlToPdf = new HtmlToPdfConverter
            {
                Size = PageSize.A4,
                Orientation = PageOrientation.Portrait
            };

            var ttfBytes = System.IO.File.ReadAllBytes("wwwroot/fonts/OpenSans-Regular.ttf");
            var base64Font = Convert.ToBase64String(ttfBytes);

            var html = $@"
            <html>
            <head>
                 <style>
                    @font-face {{
                        font-family: 'Open Sans';
                        src: url('data:font/truetype;charset=utf-8;base64,{base64Font}') format('truetype');
                        font-weight: normal;
                        font-style: normal;
                    }}
                    body {{
                        font-family: 'Open Sans', sans-serif;
                        font-size: 12px;
                        color: #000;
                        margin: 20px;
                }}
                </style>
            </head>
            <body>
                <h3 style='font-size: 16px; font-weight: bold; border: 1px solid #000; padding: 5px; margin-top: 30px;text-align:center;background-color:#d7f5cb;color:#38761d;'>{ParticularData.CompanyName}</h3>
                <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                    <tr><td style='width: 30%; font-weight: bold;'>Company Name</td><td>{ParticularData.CompanyName}</td></tr>
                    <tr><td style='font-weight: bold;'>Registred Office</td><td>{RegistredOfficeAddress.HouseName} {RegistredOfficeAddress.Street}, {RegistredOfficeAddress.Locality}, {RegistredOfficeAddress.County}, {RegistredOfficeAddress.PostCode}, {RegistredOfficeAddress.Country}</td></tr>
                    <tr><td style='font-weight: bold;'>Business Office</td><td>{BusinessOfficeAddress.HouseName} {BusinessOfficeAddress.Street}, {BusinessOfficeAddress.Locality}, {BusinessOfficeAddress.County}, {BusinessOfficeAddress.PostCode}, {BusinessOfficeAddress.Country}</td></tr>
                    <tr><td style='font-weight: bold;'>Registred Email</td><td>{RegistredEmail}</td></tr>
                    <tr><td style='font-weight: bold;'>Company Type</td><td>{ParticularData.CompanyType}</td></tr>
                    <tr><td style='font-weight: bold;'>Jurisdiction</td><td>{ParticularData.Jurisdiction}</td></tr>
                </table>
            
                <h3 style='font-size: 16px; font-weight: bold; border: 1px solid #000; padding: 5px; margin-top: 30px;text-align:center;background-color:#d7f5cb;color:#38761d;'>Appointments</h3>";


            foreach (var item in OfficersList)
            {
                var residential = PersonAddressList.FirstOrDefault(x => x.OfficerId == item.OfficerId && x.IsResidetialAddress);
                var service = PersonAddressList.FirstOrDefault(x => x.OfficerId == item.OfficerId && x.IsServiceAddress);

                html += $@"
                <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                    <tr><td style='width: 30%; font-weight: bold;'>Name</td><td>{item.FirstName} {item.LastName}</td></tr>
                    <tr><td style='font-weight: bold;'>Roles</td><td>{item.PositionName}</td></tr>
                    <tr><td style='font-weight: bold;'>DOB</td><td>{item.DOB:dd/MM/yyyy}</td></tr>
                    <tr><td style='font-weight: bold;'>Occupation</td><td>{item.Occupation}</td></tr>
                    <tr><td style='font-weight: bold;'>Nationality</td><td>{item.Nationality}</td></tr>
                    <tr><td style='font-weight: bold;'>Residential Address</td><td>{residential?.HouseName} {residential?.Street}, {residential?.Locality}, {residential?.County}, {residential?.PostCode}, {residential?.Country}</td></tr>
                    <tr><td style='font-weight: bold;'>Service Address</td><td>{service?.HouseName} {service?.Street}, {service?.Locality}, {service?.County}, {service?.PostCode}, {service?.Country}</td></tr>
                </table>";
            }

            if (UploadedDocuments?.Count > 0)
            {
                var identities = UploadedDocuments.Where(x => x.DocumentType == "identity");
                var addresses = UploadedDocuments.Where(x => x.DocumentType == "address");

                html += "<h3 style='font-size: 16px; font-weight: bold; border: 1px solid #000; padding: 5px; margin-top: 30px;text-align:center;background-color:#d7f5cb;color:#38761d;'>Documents</h3>";

                if (identities.Any())
                {
                    html += "<h4>Proof of Identity</h4><table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>";
                    foreach (var doc in identities)
                    {
                        html += $"<tr><td style='width: 30%; font-weight: bold;'>{doc.DocumentName}</td><td>{doc.FileName}</td></tr>";
                    }
                    html += "</table>";
                }

                if (addresses.Any())
                {
                    html += "<h4>Proof of Address</h4><table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>";
                    foreach (var doc in addresses)
                    {
                        html += $"<tr><td style='width: 30%; font-weight: bold;'>{doc.DocumentName}</td><td>{doc.FileName}</td></tr>";
                    }
                    html += "</table>";
                }
            }
            html += "<h3 style='font-size: 16px; font-weight: bold; border: 1px solid #000; padding: 5px; margin-top: 30px;text-align:center;background-color:#d7f5cb;color:#38761d;'>SIC Codes</h3>";
            html += "<table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>";

            foreach (var sic in SicCodesWithDesc)
            {
                html += $"<tr><td style='width: 30%; font-weight: bold;'>{sic.Code}</td><td>{sic.Description}</td></tr>";
            }

            html += "</table>";

            html += "</table></body></html>";


            byte[] pdfBytes = htmlToPdf.GeneratePdf(html);

            return File(pdfBytes, "application/pdf", $"{ParticularData.CompanyName} Summary.pdf");
        }

    }
}
