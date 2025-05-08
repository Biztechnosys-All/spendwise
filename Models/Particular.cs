using System.ComponentModel.DataAnnotations;

namespace Spendwise_WebApp.Models
{
    public class Particular
    {
        [Key]
        public int ParticularId { get; set; }
        public int? UserId {  get; set; }
        public int? CompanyId { get; set; }
        public required string CompanyName { get; set; }
        public required string CompanyType { get; set; }
        public required string Jurisdiction { get; set; }
        public required string Activities { get; set; }
        public required string SIC_Code { get; set; }
    }
}
