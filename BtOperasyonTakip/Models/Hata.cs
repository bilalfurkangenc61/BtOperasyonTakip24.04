using System.ComponentModel.DataAnnotations.Schema;

namespace BtOperasyonTakip.Models
{
    public class Hata
    {
        public int Id { get; set; }
        public string HataAdi { get; set; }
        public string HataAciklama { get; set; }
        public string KategoriBilgisi { get; set; }
        public int? OlusturanUserId { get; set; }
        public string OlusturanKullaniciAdi { get; set; }
        public DateTime OlusturmaTarihi { get; set; }
        public string Durum { get; set; } // Açık, Kapalı, Beklemede
        public int? SecilenHataId { get; set; } // Varsa mevcul hataya bağlı
        public string OperasyonCevabi { get; set; }
        public int? CevaplayanUserId { get; set; }
        public string CevaplayanKullaniciAdi { get; set; }
        public DateTime? CevaplaamaTarihi { get; set; }

        public int? MusteriID { get; set; }

        [ForeignKey(nameof(MusteriID))]
        public Musteri? Musteri { get; set; }
    }
}