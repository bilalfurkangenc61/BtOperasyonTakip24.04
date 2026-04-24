namespace BtOperasyonTakip.Models
{
    public class Detay
    {
        public int DetayID { get; set; }   
        public int MusteriID { get; set; }
        public DateTime Tarih { get; set; }
        public string? Gorusulen { get; set; }
        public string? Aciklama { get; set; }
        public string? Kekleyen { get; set; }

        public Musteri? Musteri { get; set; }
    }
}
