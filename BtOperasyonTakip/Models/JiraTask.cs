using System.ComponentModel.DataAnnotations;

namespace BtOperasyonTakip.Models
{
    public class JiraTask
    {
        [Key]
        public int Id { get; set; }

        public int? MusteriID { get; set; }

        public string? MusteriAdi { get; set; }

        [Required]
        public string JiraId { get; set; }

        [Required]
        public string TalepKonusu { get; set; }

        public string? TalepTuru { get; set; }

        public string TalepAcan { get; set; }

        public string Durum { get; set; }

        public string? TakipEden { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public ICollection<JiraYorum>? Yorumlar { get; set; }
    }
}