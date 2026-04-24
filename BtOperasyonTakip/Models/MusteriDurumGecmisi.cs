using System.ComponentModel.DataAnnotations;

namespace BtOperasyonTakip.Models
{
    public sealed class MusteriDurumGecmisi
    {
        public int Id { get; set; }
        public int MusteriID { get; set; }

        [MaxLength(200)]
        public string? EskiDurum { get; set; }

        [MaxLength(200)]
        public string? YeniDurum { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Aciklama { get; set; } = string.Empty;

        public DateTime Tarih { get; set; } = DateTime.Now;

        [MaxLength(200)]
        public string? DegistirenKullanici { get; set; }

        public Musteri? Musteri { get; set; }
    }
}
