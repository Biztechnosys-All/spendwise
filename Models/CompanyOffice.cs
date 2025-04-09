using System.ComponentModel.DataAnnotations;

namespace Spendwise_WebApp.Models
{
    public class CompanyOffice
    {
        [Key]
        public int OfficeID { get; set; }
        public required int CompanyId { get; set; }
        public required string OfficeName { get; set; }
        public required string Street { get; set; }
        public required string Locality { get; set; }
        public required string Town { get; set; }        
        public required string Country { get; set; }
        public required string PostCode { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
