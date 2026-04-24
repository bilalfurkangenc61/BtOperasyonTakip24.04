using System;
using System.ComponentModel.DataAnnotations;

namespace BtOperasyonTakip.Models
{
    public class TicketAtamaLog
    {
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }

        public int? EskiOperasyonUserId { get; set; }

        [StringLength(100)]
        public string? EskiOperasyonKullaniciAdi { get; set; }

        [Required]
        public int YeniOperasyonUserId { get; set; }

        [StringLength(100)]
        public string? YeniOperasyonKullaniciAdi { get; set; }

        [Required, StringLength(500)]
        public string DegisiklikNedeni { get; set; } = string.Empty;

        public int DegistirenUserId { get; set; }

        [StringLength(100)]
        public string? DegistirenKullaniciAdi { get; set; }

        public DateTime DegisiklikTarihi { get; set; } = DateTime.UtcNow;
    }
}