using System.ComponentModel.DataAnnotations;

namespace BtOperasyonTakip.Models
{
    public class ExcelAktarimKaydi
    {
        public int Id { get; set; }

        [MaxLength(255)]
        public string DosyaAdi { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? SayfaAdi { get; set; }

        public int SatirNo { get; set; }

        [MaxLength(255)]
        public string? MusteriAdi { get; set; }

        public DateTime? Tarih { get; set; }

        public string? Gorusulen { get; set; }

        public string? Aciklama { get; set; }

        [MaxLength(100)]
        public string? Ekleyen { get; set; }

        public int? EslesenMusteriID { get; set; }

        public Musteri? EslesenMusteri { get; set; }

        [MaxLength(100)]
        public string? YukleyenKullaniciAdi { get; set; }

        public DateTime YuklemeTarihi { get; set; } = DateTime.Now;
    }
}
