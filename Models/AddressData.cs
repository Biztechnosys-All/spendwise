using System.ComponentModel.DataAnnotations;

namespace Spendwise_WebApp.Models
{
    public class AddressData
    {
        [Key]
        public int AddressId { get; set; }
        public required string HouseName { get; set; }
        public required string Street { get; set; }
        public string? Locality { get; set; }
        public required string Town { get; set; }
        public string? County { get; set; }
        public required string Country { get; set; }
        public required string PostCode { get; set; }
        public int CompanyId { get; set; }
        public int OfficerId { get; set; }
        public int? UserId { get; set; }
        public bool IsPrimary { get; set; } = false;
        public bool IsBilling { get; set; } = false;
        public bool IsRegisteredOffice { get; set; } = false;
        public bool IsBusiness { get; set; } = false;
        public bool IsResidetialAddress { get; set; } = false;
        public bool IsServiceAddress { get; set; } = false;
    }
}
