using System.ComponentModel.DataAnnotations;

namespace BtOperasyonTakip.Models;

public class IzinTalebi
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    [Required, StringLength(100)]
    public string TalepSahibiAdi { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime BaslangicTarihi { get; set; }

    [DataType(DataType.Date)]
    public DateTime BitisTarihi { get; set; }

    [Range(1, 365)]
    public int GunSayisi { get; set; }

    [StringLength(1000)]
    public string? Aciklama { get; set; }

    [Required, StringLength(20)]
    public string Durum { get; set; } = IzinTalepDurumlari.Beklemede;

    [StringLength(1000)]
    public string? AdminAciklama { get; set; }

    public DateTime TalepTarihi { get; set; } = DateTime.Now;
    public DateTime GuncellemeTarihi { get; set; } = DateTime.Now;
    public DateTime? KararTarihi { get; set; }

    public int? KararVerenUserId { get; set; }

    [StringLength(100)]
    public string? KararVerenAdi { get; set; }
}
