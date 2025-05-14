namespace Spendwise_WebApp.Models
{
    public class Document
    {
        public int DocumentId { get; set; }
        public int? UserId { get; set; }
        public int? CompanyId { get; set; }
        public required string FileName { get; set; }
        public required string FilePath { get; set; }
        public required string DocumentType { get; set; } // Identity or Address
        public required string DocumentName { get; set; } // e.g., Passport, Driving Licence
        public DateTime UploadedOn { get; set; }
    }
}
