namespace BtOperasyonTakip.Models
{
    public class JiraBoardViewModel
    {
        public List<JiraTask> Beklemede { get; set; } = new();
        public List<JiraTask> Aktif { get; set; } = new();
        public List<JiraTask> Tamamlandi { get; set; } = new();
        public List<Musteri> MusteriSecenekleri { get; set; } = new();

        public List<string> TalepAcanSecenekleri { get; set; } = new();
        public List<string> TakipEdenSecenekleri { get; set; } = new();

        public string? Q { get; set; }
        public int? SelectedTaskId { get; set; }
        public JiraTask? SelectedTask { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }

        public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}