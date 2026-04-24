using System.Collections.Generic;

namespace BtOperasyonTakip.Models
{
    public class HataIssueViewModel
    {
        public List<Issue> Issues { get; set; } = new();
        public List<Hata> Hatalar { get; set; } = new();
    }
}