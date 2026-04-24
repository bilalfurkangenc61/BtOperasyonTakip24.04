using System;
using System.Collections.Generic;

namespace BtOperasyonTakip.Models
{
    public class DashboardViewModel
    {
        public int ToplamMusteri { get; set; }
        public int AktifMusteri { get; set; }
        public int PasifMusteri { get; set; }
        public int BuAyEklenen { get; set; }
        public int Bekleyen { get; set; }

        public List<int> AylikMusteriSayilari { get; set; } = new();
        public List<string> AyEtiketleri { get; set; } = new();

        public int JiraBeklemede { get; set; }
        public int JiraAktif { get; set; }
        public int JiraTamamlandi { get; set; }

        public int TicketOnaybekleniyor { get; set; }
        public int TicketOnaylandi { get; set; }
        public int TicketReddedildi { get; set; }
        public int ToplamTicket { get; set; }

        public List<string> MusteriDurumEtiketleri { get; set; } = new();
        public List<int> MusteriDurumSayilari { get; set; } = new();

        public List<Musteri> Musteriler { get; set; } = new();
        public List<JiraTask> JiraTasks { get; set; } = new();
        public List<Ticket> Tickets { get; set; } = new();
        public List<ToplantiNotu> ToplantiNotlari { get; set; } = new();

        public int SeciliAy { get; set; }
        public int SeciliYil { get; set; }
        public List<(int Year, int Month, string Label)> AySecenekleri { get; set; } = new();

        public int HataToplamSeciliAy { get; set; }
        public int HataAcikSeciliAy { get; set; }
        public int HataBeklemedeSeciliAy { get; set; }
        public int HataKapaliSeciliAy { get; set; }
    }
}