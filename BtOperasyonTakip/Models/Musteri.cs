using System.ComponentModel.DataAnnotations;

namespace BtOperasyonTakip.Models
{
    public class Musteri
    {
        public int MusteriID { get; set; }
        public string? Firma { get; set; }
        public string? SiteUrl { get; set; }
        public string? Teknoloji { get; set; }
        public string? Durum { get; set; }
        public DateTime? DurumDegisiklikTarihi { get; set; }
        public DateTime? DokumanKontrolBaslangicTarihi { get; set; }
        public int? DokumanGonderimSayisi { get; set; }
        public bool? DokumanGonderildiMi { get; set; }
        public string? TalepSahibi { get; set; }

        [MaxLength(20, ErrorMessage = "Telefon numarası 20 karakterden uzun olamaz.")]
        public string? Telefon { get; set; }

        public string? Aciklama { get; set; }
        public ICollection<Detay>? Detaylar { get; set; }
        public ICollection<MusteriDurumGecmisi>? DurumGecmisi { get; set; }
        public DateTime? KayitTarihi { get; set; } = DateTime.Now;

        public string? FirmaYetkilisi { get; set; }

        [MaxLength(50)]
        public string? Kaynak { get; set; } // "Kurumsal" / null (eski kayıtlar)
    }
}