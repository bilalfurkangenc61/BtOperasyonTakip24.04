namespace BtOperasyonTakip.Models
{
    public class AjaxResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T Data { get; set; }
        public int JiraBeklemede { get; set; }
        public int JiraAktif { get; set; }
        public int JiraTamamlandi { get; set; }

    }
}
