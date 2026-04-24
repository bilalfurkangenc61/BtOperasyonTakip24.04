namespace BtOperasyonTakip.Models.ViewModels
{
    public sealed class HataListItemVm
    {
        public int Id { get; init; }
        public string HataAdi { get; init; } = "";
        public string KategoriBilgisi { get; init; } = "";
        public string OlusturanKullaniciAdi { get; init; } = "";
        public string Durum { get; init; } = "";
        public DateTime OlusturmaTarihi { get; init; }

        public string? MusteriFirma { get; init; }
    }
}