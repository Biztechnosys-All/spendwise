using System.ComponentModel.DataAnnotations;

namespace Spendwise_WebApp.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        public string Password { get; set; }
        public string MobileNumber { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public DateTime? created_on { get; set; }
        public int created_by { get; set; }
    }
}
