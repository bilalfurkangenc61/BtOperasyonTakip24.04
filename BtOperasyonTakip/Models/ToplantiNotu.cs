using System;
using System.ComponentModel.DataAnnotations;

namespace BtOperasyonTakip.Models
{
    public class ToplantiNotu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MusteriAdi { get; set; } = string.Empty;

        [Required]
        public string NotIcerigi { get; set; } = string.Empty;

        public string EkleyenKisi { get; set; }

        public DateTime Tarih { get; set; } = DateTime.Now;

        public string ToplantiBasligi { get; set; }
        public string Konum { get; set; }
        public string Saat { get; set; }
        public string Hazirlayan { get; set; }
        public string Katilimcilar { get; set; }
        public string Konu { get; set; }

        public string? Tur { get; set; }
        public string? AksiyonSahibi { get; set; }
        public string? HedefTarihi { get; set; }
    }
}
