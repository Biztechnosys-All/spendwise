using System.ComponentModel.DataAnnotations;

namespace Spendwise_WebApp.Models
{
    public class CompanyOfficer
    {
        [Key]
        public int OfficerId { get; set; }
        public int? CompanyID { get; set; }
        public int? UserId { get; set; }
        public string? Title { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DOB { get; set; }
        public string? Nationality { get; set; }
        public string? Occupation { get; set; }
        public string? PositionName { get; set; }
        public string? Authentication1 { get; set; }
        public string? AuthenticationAns1 { get; set; }
        public string? Authentication2 { get; set; }
        public string? AuthenticationAns2 { get; set; }
        public string? Authentication3 { get; set; }
        public string? AuthenticationAns3 { get; set; }
    }
    
}
