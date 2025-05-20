using System.ComponentModel.DataAnnotations;

namespace Spendwise_WebApp.Models
{
    public class OtherLegalOfficers
    {
        [Key]
        public int LegalOfficerId { get; set; }
        public int? CompanyID { get; set; }
        public int? UserId { get; set; }
        public string? LegalName { get; set; }
        public string? LawGoverned { get; set; }
        public string? LegalForm { get; set; }
        public string? PositionName { get; set; }
    }
}
