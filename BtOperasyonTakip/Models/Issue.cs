using System;

namespace BtOperasyonTakip.Models
{
    public enum IssueStatus
    {
        Bekleme,
        Acik,
        Kapandi
    }

    public class Issue
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Reporter { get; set; } = "";
        public IssueStatus Status { get; set; } = IssueStatus.Bekleme;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}