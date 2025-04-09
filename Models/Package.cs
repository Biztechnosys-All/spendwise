using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Spendwise_WebApp.Models
{
    public class Package
    {
        [Key]
        public int PackageId { get; set; }
        [Required]
        public string PackageName { get; set; }
        [Required]
        [DisplayName("Short Description")]
        public string ShortDescription { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        [DisplayName("Package Features")]
        public string PackageFeatures { get; set; }
        [DisplayName("Is Limited Company")]
        public bool IsLimitedCompanyPkg { get; set; }
        public DateTime? created_on { get; set; }
    }
}
