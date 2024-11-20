using System.ComponentModel.DataAnnotations;

namespace Spendwise_WebApp.Models
{
    public class PackageFeature
    {
        [Key]
        public int FeatureId { get; set; }
        [Required]
        public string Feature { get; set; }
    }
}
