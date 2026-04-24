using System;
using System.ComponentModel.DataAnnotations;

namespace BtOperasyonTakip.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string FirmaAdi { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string MusteriWebSitesi { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string YazilimciAdi { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string YazilimciSoyadi { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string IrtibatNumarasi { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(100)]
        public string Mail { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string MusteriTipi { get; set; } = "Kurumsal";

        [StringLength(100)]
        public string? TeknolojiBilgisi { get; set; }

        [StringLength(1000)]
        public string? Aciklama { get; set; }

        [Required, StringLength(50)]
        public string Durum { get; set; } = "Operasyon 1 Onay Bekleniyor";

        [StringLength(500)]
        public string? KararAciklamasi { get; set; }

        [Required]
        public int OlusturanUserId { get; set; }

        [StringLength(100)]
        public string? OlusturanKullaniciAdi { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        public int? OnaylayanUserId { get; set; }

        [StringLength(100)]
        public string? OnaylayanKullaniciAdi { get; set; }

        public DateTime? OnaylamaTarihi { get; set; }

        public int? MusteriID { get; set; }

        public int? UyumOnaylayanUserId { get; set; }

        [StringLength(100)]
        public string? UyumOnaylayanKullaniciAdi { get; set; }

        public DateTime? UyumOnayTarihi { get; set; }

        [StringLength(500)]
        public string? UyumKararAciklamasi { get; set; }

        public bool? EntegreOlabilirMi { get; set; }

        [StringLength(250)]
        public string? EntegrasyonNotu { get; set; }

        public DateTime? Operasyon1OnayTarihi { get; set; }

        public bool? MailGonderildiMi { get; set; }

        [StringLength(250)]
        public string? MailNotu { get; set; }

        public DateTime? Operasyon2OnayTarihi { get; set; }

        public DateTime? CanliAcildiTarihi { get; set; }

        [StringLength(250)]
        public string? CanliNotu { get; set; }

        public int? AtananOperasyonUserId { get; set; }

        [StringLength(100)]
        public string? AtananOperasyonKullaniciAdi { get; set; }

        public DateTime? AtanmaTarihi { get; set; }
        
        [StringLength(100)]
        public string? Kategori { get; set; }

        [StringLength(100)]
        public string? WebSitesiTipi { get; set; }

        [StringLength(200)]
        public string? OdemeTurleri { get; set; }

        [StringLength(200)]
        public string? OdemeYontemleri { get; set; }
    }
}