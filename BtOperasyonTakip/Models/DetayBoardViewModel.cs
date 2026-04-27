namespace BtOperasyonTakip.Models
{
    public class DetayBoardViewModel
    {
        public List<Musteri> Musteriler { get; set; } = new();
        public Dictionary<int, Detay> SonDetaylar { get; set; } = new();
        public List<string> TeknolojiSecenekleri { get; set; } = new();
        public List<string> TalepSahibiSecenekleri { get; set; } = new();
        public List<string> KullaniciSecenekleri { get; set; } = new();

        public string? Q { get; set; }
        public string? Durum { get; set; }
        public string? Teknoloji { get; set; }
        public string? TalepSahibi { get; set; }
        public string? MinDate { get; set; }
        public string? MaxDate { get; set; }
        public int? SelectedMusteriId { get; set; }
        public string? SelectedMusteriAdi { get; set; }
        public List<Detay> SelectedMusteriDetaylar { get; set; } = new();

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }

        public int AktifCount { get; set; }
        public int PasifCount { get; set; }
        public int BeklemedeCount { get; set; }

        public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
