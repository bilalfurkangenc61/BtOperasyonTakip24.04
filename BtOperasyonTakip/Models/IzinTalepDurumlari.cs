namespace BtOperasyonTakip.Models;

public static class IzinTalepDurumlari
{
    public const string Beklemede = "Beklemede";
    public const string Onaylandi = "Onaylandı";
    public const string Reddedildi = "Reddedildi";

    public static readonly string[] TumDurumlar = [Beklemede, Onaylandi, Reddedildi];
}
