using System.ComponentModel.DataAnnotations;

namespace Spendwise_WebApp.Models
{
    public class Orders
    {
        [Key]
        public int OrderId { get; set; }
        public int OrderBy { get; set; }
        public string OrderByIP {  get; set; }
        public required DateTime OrderDate { get; set; }
        public required int PackageID { get; set; }
        public required string PackageName { get; set; }
        public required string CompanyName { get; set; }
        public int CompanyId { get; set; }
        public string AdditionalPackageItemIds { get; set; } 
        public required double NetAmount { get; set; }
        public required double VatAmount { get; set; }
        public required double TotalAmount { get; set; }
        public required double AmountDue { get; set; }
        public bool IsOrderComplete { get; set; }
        public DateTime? InvoicedDate { get; set; }
        
    }
}
