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
        public string Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public string PackageFeatures { get; set; }
        public DateTime? created_on { get; set; }
    }
}
