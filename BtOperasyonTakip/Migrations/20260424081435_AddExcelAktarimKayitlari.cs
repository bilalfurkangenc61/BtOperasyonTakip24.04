using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BtOperasyonTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddExcelAktarimKayitlari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExcelAktarimKayitlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DosyaAdi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SayfaAdi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SatirNo = table.Column<int>(type: "int", nullable: false),
                    MusteriAdi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gorusulen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ekleyen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EslesenMusteriID = table.Column<int>(type: "int", nullable: true),
                    YukleyenKullaniciAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    YuklemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcelAktarimKayitlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExcelAktarimKayitlari_Musteriler_EslesenMusteriID",
                        column: x => x.EslesenMusteriID,
                        principalTable: "Musteriler",
                        principalColumn: "MusteriID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExcelAktarimKayitlari_EslesenMusteriID",
                table: "ExcelAktarimKayitlari",
                column: "EslesenMusteriID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExcelAktarimKayitlari");
        }
    }
}
