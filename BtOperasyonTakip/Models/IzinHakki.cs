using System.ComponentModel.DataAnnotations;

namespace BtOperasyonTakip.Models;

public class IzinHakki
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    [Range(0, 365)]
    public int ToplamGun { get; set; }

    public DateTime GuncellemeTarihi { get; set; } = DateTime.Now;

    [StringLength(100)]
    public string? GuncelleyenAdi { get; set; }
}
