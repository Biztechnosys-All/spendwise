﻿using System.ComponentModel.DataAnnotations;

namespace Spendwise_WebApp.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        public required string Title { get; set; }
        [Required]
        public required string Forename { get; set; }
        [Required]
        public required string Surname { get; set; }
        [Required]
        public required string PhoneNumber { get; set; }
        [Required]
        public required string Email { get; set; }
        public string? BillingPhoneNumber { get; set; }
        public string? BillingEmail { get; set; }
        [Required]
        public required string Password { get; set; }
        public string? EmailVerificationToken { get; set; }
        public bool IsEmailVerified { get; set; }
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }

        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public DateTime? created_on { get; set; }
        public int created_by { get; set; }
    }
}
