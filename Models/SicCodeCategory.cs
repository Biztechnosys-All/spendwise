using System.ComponentModel.DataAnnotations;

namespace Spendwise_WebApp.Models
{
    public class SicCodeCategory
    {
        [Key]
        public int SicCatId { get; set; }
        public string Section { get; set; }
        public string Category { get; set; } 
    }
}
