namespace BtOperasyonTakip.Models
{
    public class ExcelAktarimIndexViewModel
    {
        public List<ExcelAktarimKaydi> Kayitlar { get; set; } = new();

        public string? Q { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 25;

        public int TotalCount { get; set; }

        public int EslesenCount { get; set; }

        public int EslesmeyenCount { get; set; }

        public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
