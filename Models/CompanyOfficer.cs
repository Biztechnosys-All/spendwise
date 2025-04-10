using System.ComponentModel.DataAnnotations;

namespace Spendwise_WebApp.Models
{
    public class CompanyOfficer
    {
        [Key]
        public int OfficerId { get; set; }
        public int CompanyID { get; set; }
        public required string Title { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required DateTime DOB { get; set; }
        public string Nationality { get; set; }
        public string Occupation { get; set; }
        public required string HouseName { get; set; }
        public required string Street { get; set; }
        public required string Locality { get; set; }
        public required string Town { get; set; }
        public required string Country { get; set; }
        public required string PostCode { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
