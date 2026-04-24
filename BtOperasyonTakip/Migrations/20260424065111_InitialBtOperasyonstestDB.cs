using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BtOperasyonTakip.Migrations
{
    /// <inheritdoc />
    public partial class InitialBtOperasyonstestDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IletisimBilgileri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdSoyad = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IletisimBilgileri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Issues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reporter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JiraTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MusteriID = table.Column<int>(type: "int", nullable: true),
                    MusteriAdi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JiraId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TalepKonusu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TalepTuru = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TalepAcan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TakipEden = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JiraTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Musteriler",
                columns: table => new
                {
                    MusteriID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Firma = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SiteUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Teknoloji = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Durum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DurumDegisiklikTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DokumanKontrolBaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DokumanGonderimSayisi = table.Column<int>(type: "int", nullable: true),
                    DokumanGonderildiMi = table.Column<bool>(type: "bit", nullable: true),
                    TalepSahibi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FirmaYetkilisi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Kaynak = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Musteriler", x => x.MusteriID);
                });

            migrationBuilder.CreateTable(
                name: "Parametreler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParAdi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tur = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parametreler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketAtamaLoglari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    EskiOperasyonUserId = table.Column<int>(type: "int", nullable: true),
                    EskiOperasyonKullaniciAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    YeniOperasyonUserId = table.Column<int>(type: "int", nullable: false),
                    YeniOperasyonKullaniciAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DegisiklikNedeni = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DegistirenUserId = table.Column<int>(type: "int", nullable: false),
                    DegistirenKullaniciAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DegisiklikTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketAtamaLoglari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirmaAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MusteriWebSitesi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    YazilimciAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    YazilimciSoyadi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IrtibatNumarasi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Mail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MusteriTipi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TeknolojiBilgisi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Durum = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KararAciklamasi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OlusturanUserId = table.Column<int>(type: "int", nullable: false),
                    OlusturanKullaniciAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OnaylayanUserId = table.Column<int>(type: "int", nullable: true),
                    OnaylayanKullaniciAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OnaylamaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MusteriID = table.Column<int>(type: "int", nullable: true),
                    UyumOnaylayanUserId = table.Column<int>(type: "int", nullable: true),
                    UyumOnaylayanKullaniciAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UyumOnayTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UyumKararAciklamasi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EntegreOlabilirMi = table.Column<bool>(type: "bit", nullable: true),
                    EntegrasyonNotu = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Operasyon1OnayTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MailGonderildiMi = table.Column<bool>(type: "bit", nullable: true),
                    MailNotu = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Operasyon2OnayTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CanliAcildiTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CanliNotu = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    AtananOperasyonUserId = table.Column<int>(type: "int", nullable: true),
                    AtananOperasyonKullaniciAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AtanmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Kategori = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WebSitesiTipi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OdemeTurleri = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OdemeYontemleri = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ToplantiNotlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MusteriAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NotIcerigi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EkleyenKisi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToplantiBasligi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Konum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Saat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Hazirlayan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Katilimcilar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Konu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tur = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AksiyonSahibi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HedefTarihi = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToplantiNotlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JiraYorumlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JiraTaskId = table.Column<int>(type: "int", nullable: false),
                    YorumMetni = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ekleyen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JiraYorumlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JiraYorumlar_JiraTasks_JiraTaskId",
                        column: x => x.JiraTaskId,
                        principalTable: "JiraTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Detaylar",
                columns: table => new
                {
                    DetayID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MusteriID = table.Column<int>(type: "int", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gorusulen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Kekleyen = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Detaylar", x => x.DetayID);
                    table.ForeignKey(
                        name: "FK_Detaylar_Musteriler_MusteriID",
                        column: x => x.MusteriID,
                        principalTable: "Musteriler",
                        principalColumn: "MusteriID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hatalar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HataAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HataAciklama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KategoriBilgisi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OlusturanUserId = table.Column<int>(type: "int", nullable: true),
                    OlusturanKullaniciAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SecilenHataId = table.Column<int>(type: "int", nullable: true),
                    OperasyonCevabi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CevaplayanUserId = table.Column<int>(type: "int", nullable: true),
                    CevaplayanKullaniciAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CevaplaamaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MusteriID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hatalar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hatalar_Musteriler_MusteriID",
                        column: x => x.MusteriID,
                        principalTable: "Musteriler",
                        principalColumn: "MusteriID");
                });

            migrationBuilder.CreateTable(
                name: "MusteriDurumGecmisleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MusteriID = table.Column<int>(type: "int", nullable: false),
                    EskiDurum = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    YeniDurum = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DegistirenKullanici = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusteriDurumGecmisleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MusteriDurumGecmisleri_Musteriler_MusteriID",
                        column: x => x.MusteriID,
                        principalTable: "Musteriler",
                        principalColumn: "MusteriID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IzinHaklari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ToplamGun = table.Column<int>(type: "int", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncelleyenAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IzinHaklari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IzinHaklari_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IzinTalepleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TalepSahibiAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GunSayisi = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Durum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AdminAciklama = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TalepTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KararTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KararVerenUserId = table.Column<int>(type: "int", nullable: true),
                    KararVerenAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IzinTalepleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IzinTalepleri_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Detaylar_MusteriID",
                table: "Detaylar",
                column: "MusteriID");

            migrationBuilder.CreateIndex(
                name: "IX_Hatalar_MusteriID",
                table: "Hatalar",
                column: "MusteriID");

            migrationBuilder.CreateIndex(
                name: "IX_IzinHaklari_UserId",
                table: "IzinHaklari",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IzinTalepleri_UserId",
                table: "IzinTalepleri",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_JiraYorumlar_JiraTaskId",
                table: "JiraYorumlar",
                column: "JiraTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_MusteriDurumGecmisleri_MusteriID",
                table: "MusteriDurumGecmisleri",
                column: "MusteriID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Detaylar");

            migrationBuilder.DropTable(
                name: "Hatalar");

            migrationBuilder.DropTable(
                name: "IletisimBilgileri");

            migrationBuilder.DropTable(
                name: "Issues");

            migrationBuilder.DropTable(
                name: "IzinHaklari");

            migrationBuilder.DropTable(
                name: "IzinTalepleri");

            migrationBuilder.DropTable(
                name: "JiraYorumlar");

            migrationBuilder.DropTable(
                name: "MusteriDurumGecmisleri");

            migrationBuilder.DropTable(
                name: "Parametreler");

            migrationBuilder.DropTable(
                name: "TicketAtamaLoglari");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "ToplantiNotlari");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "JiraTasks");

            migrationBuilder.DropTable(
                name: "Musteriler");
        }
    }
}
