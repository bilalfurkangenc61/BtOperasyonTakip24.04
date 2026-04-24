using System;
using System.ComponentModel.DataAnnotations;

namespace BtOperasyonTakip.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? FullName { get; set; } 


        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required, StringLength(20)]
        public string Role { get; set; } = "Saha";
    }
}
