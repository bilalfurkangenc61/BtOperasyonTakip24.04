using System.ComponentModel.DataAnnotations;

namespace BtOperasyonTakip.Models;

public class IzinTakipViewModel
{
    public bool IsAdmin { get; set; }
    public int CurrentUserId { get; set; }
    public string CurrentUserName { get; set; } = string.Empty;
    public int ToplamHak { get; set; }
    public int KullanilanGun { get; set; }
    public int KalanGun { get; set; }
    public int BekleyenTalepSayisi { get; set; }
    public int OnaylananTalepSayisi { get; set; }
    public int ReddedilenTalepSayisi { get; set; }
    public IzinTalepFormViewModel Form { get; set; } = new();
    public List<IzinTalebiSatirViewModel> Talepler { get; set; } = [];
    public List<IzinHakkiSatirViewModel> KullaniciHaklari { get; set; } = [];
}

public class IzinTalepFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
    [DataType(DataType.Date)]
    public DateTime? BaslangicTarihi { get; set; }

    [Required(ErrorMessage = "Bitiş tarihi zorunludur.")]
    [DataType(DataType.Date)]
    public DateTime? BitisTarihi { get; set; }

    [StringLength(1000)]
    public string? Aciklama { get; set; }
}

public class IzinTalebiSatirViewModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string TalepSahibiAdi { get; set; } = string.Empty;
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public int GunSayisi { get; set; }
    public string? Aciklama { get; set; }
    public string Durum { get; set; } = IzinTalepDurumlari.Beklemede;
    public string? AdminAciklama { get; set; }
    public string? KararVerenAdi { get; set; }
    public DateTime TalepTarihi { get; set; }
    public DateTime? KararTarihi { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDecide { get; set; }
}

public class IzinHakkiSatirViewModel
{
    public int UserId { get; set; }
    public string KullaniciAdi { get; set; } = string.Empty;
    public int ToplamGun { get; set; }
    public int KullanilanGun { get; set; }
    public int KalanGun { get; set; }
}
