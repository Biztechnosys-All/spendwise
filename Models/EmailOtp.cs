namespace Spendwise_WebApp.Models
{
    public class EmailOtp
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string OTP { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
