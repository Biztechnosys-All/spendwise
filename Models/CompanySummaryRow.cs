namespace Spendwise_WebApp.Models
{
    public class CompanySummaryRow
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string RegisteredEmail { get; set; }
        public string Jurisdiction { get; set; }
        public string CompanyType { get; set; }
        public string SIC_Code { get; set; }

        public string Reg_HouseName { get; set; }
        public string Reg_Street { get; set; }
        public string Reg_Locality { get; set; }
        public string Reg_County { get; set; }
        public string Reg_PostCode { get; set; }
        public string Reg_Country { get; set; }

        public string Bus_HouseName { get; set; }
        public string Bus_Street { get; set; }
        public string Bus_Locality { get; set; }
        public string Bus_County { get; set; }
        public string Bus_PostCode { get; set; }
        public string Bus_Country { get; set; }

        public int? OfficerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PositionName { get; set; }
        public DateTime DOB { get; set; }
        public string Occupation { get; set; }
        public string Nationality { get; set; }

        public bool? IsResidetialAddress { get; set; }
        public bool? IsServiceAddress { get; set; }
        public string Addr_HouseName { get; set; }
        public string Addr_Street { get; set; }
        public string Addr_Locality { get; set; }
        public string Addr_County { get; set; }
        public string Addr_PostCode { get; set; }
        public string Addr_Country { get; set; }

        public string DocumentName { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }
}
