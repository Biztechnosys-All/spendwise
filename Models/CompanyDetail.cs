using System.ComponentModel.DataAnnotations;

namespace Spendwise_WebApp.Models
{
    public class CompanyDetail
    {
        [Key]
        public int CompanyId { get; set; }
        public required string CompanyName { get; set; }
        public string? CompanyNumber { get; set; }
        public string? CompanyAuthCode { get; set; }
        public required string CompanyType { get; set; }
        public string? CompanyJurisdiction { get; set; }
        public required string CompanyStatus { get; set; }
        public DateTime? Createdon { get; set; }
        public required int Createdby { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? RegisteredEmail { get; set; }
    }
}
