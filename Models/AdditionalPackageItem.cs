using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spendwise_WebApp.Models
{
    public class AdditionalPackageItem
    {
        [Key]
        public int AdditionalPackageItemId { get; set; }
        [Required]
        [DisplayName("package")]
        public string PackageName { get; set; }
        [Required]
        [DisplayName("Item Name")]
        public string ItemName { get; set; }
        [Required]
        [DisplayName("Item Short Description")]
        public string ItemShortDesc { get; set; }
        [Required]
        [DisplayName("Item Details")]
        public string ItemDetail { get; set; }
        [Required]
        [DisplayName("Price")]
        public decimal price { get; set; }
        [Required]
        [DisplayName("Item Group")]
        public string ItemGroup { get; set; }
    }
}
