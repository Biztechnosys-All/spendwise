using System.ComponentModel.DataAnnotations;

namespace Spendwise_WebApp.Models
{
    public class SicCodes
    {
        [Key]
        public int SicCodeID { get; set; }
        public string Section { get; set; }
        public int SicCode { get; set; }
        public string Description { get; set; }
    }
}
