namespace Spendwise_WebApp.Models
{
    public class CompanySummaryViewModel
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string RegisteredEmail { get; set; }
        public string Jurisdiction { get; set; }
        public string CompanyType { get; set; }
        public string SicCode { get; set; }

        public List<(string Code, string Description)> SicCodesWithDesc { get; set; }

        // Addresses
        public string RegisteredOfficeAddress { get; set; }
        public string BusinessOfficeAddress { get; set; }

        // Officers
        public List<OfficerSummary> Officers { get; set; } = new List<OfficerSummary>();

        // Documents
        public List<DocumentSummary> Documents { get; set; } = new List<DocumentSummary>();
    }

    public class OfficerSummary
    {
        public int OfficerId { get; set; }
        public string FullName { get; set; }
        public string PositionName { get; set; }
        public DateTime DOB { get; set; }
        public string Occupation { get; set; }
        public string Nationality { get; set; }
        public string ResidentialAddress { get; set; }
        public string ServiceAddress { get; set; }
    }

    public class DocumentSummary
    {
        public string DocumentName { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }
}
