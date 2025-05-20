using System.ComponentModel.DataAnnotations;

namespace Spendwise_WebApp.Models
{
    public class CorporateCompanyOfficers
    {
        [Key]
        public int CorporateOfficerId { get; set; }
        public int? CompanyID { get; set; }
        public int? UserId { get; set; }
        public string? Title { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? LegalName { get; set; }
        public string? RegisteredInUK { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? PlaceRegistered { get; set; }
        public string? RegistryHeld { get; set; }
        public string? LawGoverned { get; set; }
        public string? LegalForm { get; set; }
        public string? PositionName { get; set; }
    }
}
