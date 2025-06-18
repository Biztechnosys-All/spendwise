using System;
using System.ComponentModel.DataAnnotations;

namespace Spendwise_WebApp.Models
{
    public class InvoiceHistory
    {
        [Key]
        public int InvoiceId { get; set; }
        public int InvoiceBy { get; set; }
        public int OrderId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int PackageId { get; set; }
        public required string PackageName { get; set; }
        public required string CompanyName { get; set; }
        public int? CompanyId { get; set; }
        public required string AdditionalPackageItemIds { get; set; }
        public decimal NetAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountDue { get; set; }
    }
}
