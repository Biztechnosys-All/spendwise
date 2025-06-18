using System.Text.Json.Serialization;

namespace Spendwise_WebApp.Models
{
    public class CreateTestCompanyRequestBody
    {
        [JsonPropertyName("company_name")]
        public string CompanyName { get; set; }

        [JsonPropertyName("company_type")]
        public string CompanyType { get; set; } = "llp";

        [JsonPropertyName("jurisdiction")]
        public string Jurisdiction { get; set; } = "england-wales";

        [JsonPropertyName("company_status")]
        public string CompanyStatus { get; set; } = "active";

        [JsonPropertyName("company_status_detail")]
        public string CompanyStatusDetail { get; set; } = "active";

        [JsonPropertyName("accounts_due_status")]
        public string AccountsDueStatus { get; set; } = "due-soon";

        [JsonPropertyName("has_super_secure_pscs")]
        public bool HasSuperSecurePscs { get; set; } = false;

        [JsonPropertyName("number_of_appointments")]
        public int NumberOfAppointments { get; set; } = 1;

        [JsonPropertyName("officer_roles")]
        public List<string> OfficerRoles { get; set; } = new List<string> { "director" };

        [JsonPropertyName("subtype")]
        public string SubType { get; set; } = "none";
    }

    public class CreateTestCompanyResponse
    {
        [JsonPropertyName("company_number")]
        public string CompanyNumber { get; set; }
        [JsonPropertyName("company_name")]
        public string CompanyName { get; set; }
        [JsonPropertyName("auth_code")]
        public string CompanyAuthCode { get; set; }
    }
}
