using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BtOperasyonTakip.Models
{
    public class JiraYorum
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("JiraTask")]
        public int JiraTaskId { get; set; }

        [Required]
        public string YorumMetni { get; set; }

        public string Ekleyen { get; set; } = "Sistem";

        public DateTime Tarih { get; set; } = DateTime.Now;

        public JiraTask JiraTask { get; set; } = null!;
    }
}
