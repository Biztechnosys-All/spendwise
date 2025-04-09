using System.ComponentModel.DataAnnotations;

namespace Spendwise_WebApp.Models
{
    public class CompanyDetails
    {
        [Key]
        public int CompanyId { get; set; }
        public required string CompanyName { get; set; }
        public string CompanyNumber { get; set; }
        public string CompanyAuthCode { get; set; }
        public required string CompanyType { get; set; }
        public string CompanyJurisdiction { get; set; }
        public required string CompanyStatus { get; set; }
        public required DateTime Createdon { get; set; }
        public required string Createdby { get; set; }
        public DateTime ApprovedDate { get; set; }

    }
}
