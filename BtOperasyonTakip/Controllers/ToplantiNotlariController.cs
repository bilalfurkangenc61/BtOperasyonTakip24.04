using BtOperasyonTakip.Data;
using BtOperasyonTakip.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace BtOperasyonTakip.Controllers
{
    public class ToplantiNotlariController : Controller
    {
        private readonly AppDbContext _context;

        public ToplantiNotlariController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string q)
        {
            q = (q ?? string.Empty).Trim();
            ViewBag.Q = q;

            var notlarQuery = _context.ToplantiNotlari.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                notlarQuery = notlarQuery.Where(x =>
                    (x.MusteriAdi ?? "").Contains(q) ||
                    (x.EkleyenKisi ?? "").Contains(q) ||
                    (x.NotIcerigi ?? "").Contains(q));
            }

            var notlar = notlarQuery
                .OrderByDescending(x => x.Tarih)
                .ToList();

            var toplantiTurleri = _context.Parametreler
                .Where(p => p.Tur == "ToplantiTuru" && p.ParAdi != null && p.ParAdi != "")
                .OrderBy(p => p.ParAdi)
                .Select(p => p.ParAdi!)
                .ToList();

            var aksiyonSahipleri = _context.Users
                .OrderBy(u => u.FullName ?? u.UserName)
                .ToList();

            ViewBag.ToplantiTurleri = toplantiTurleri;
            ViewBag.AksiyonSahipleri = aksiyonSahipleri;

            return View(notlar);
        }

        [HttpPost]
        public IActionResult Create()
        {
            var dateStr = Request.Form["Tarih"].ToString();
            var timeStr = Request.Form["Saat"].ToString();
            DateTime tarih = DateTime.Now;

            if (DateTime.TryParse(dateStr, out var parsedDate))
            {
                if (!string.IsNullOrWhiteSpace(timeStr))
                {
                    if (TimeSpan.TryParse(timeStr, out var ts))
                    {
                        tarih = parsedDate.Date.Add(ts);
                    }
                    else if (DateTime.TryParse($"{dateStr} {timeStr}", out var dtWhole))
                    {
                        tarih = dtWhole;
                    }
                    else
                    {
                        tarih = parsedDate.Date;
                    }
                }
                else
                {
                    tarih = parsedDate.Date;
                }
            }
            else
            {
                tarih = DateTime.Now;
            }

            var model = new ToplantiNotu
            {
                ToplantiBasligi = Request.Form["ToplantiBasligi"],
                Konum = Request.Form["Konum"],
                Tarih = tarih,
                Saat = string.IsNullOrWhiteSpace(timeStr) ? tarih.ToString("HH:mm") : timeStr,
                Hazirlayan = Request.Form["Hazirlayan"],
                Katilimcilar = Request.Form["Katilimcilar"],
                Tur = string.IsNullOrWhiteSpace(Request.Form["Tur"].ToString()) ? null : Request.Form["Tur"].ToString(),
                AksiyonSahibi = string.IsNullOrWhiteSpace(Request.Form["AksiyonSahibi"].ToString()) ? null : Request.Form["AksiyonSahibi"].ToString(),
                HedefTarihi = string.IsNullOrWhiteSpace(Request.Form["HedefTarihi"].ToString()) ? null : Request.Form["HedefTarihi"].ToString(),
                Konu = Request.Form["Konu"],
                MusteriAdi = Request.Form["ToplantiBasligi"],
                NotIcerigi = Request.Form["Konu"],
                EkleyenKisi = Request.Form["Hazirlayan"]
            };

            if (!ModelState.IsValid)
            {
                TempData["ToplantiError"] = "Kayıt doğrulanamadı.";
                return RedirectToAction("Index");
            }

            _context.ToplantiNotlari.Add(model);
            _context.SaveChanges();
            TempData["ToplantiOk"] = "Toplantı kaydı oluşturuldu.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateContent(int id, string notIcerigi, string? returnUrl)
        {
            var not = _context.ToplantiNotlari.Find(id);
            if (not == null)
            {
                TempData["ToplantiError"] = "Kayıt bulunamadı";
                return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? LocalRedirect(returnUrl)
                    : RedirectToAction(nameof(Index));
            }

            not.NotIcerigi = notIcerigi;
            _context.SaveChanges();

            TempData["ToplantiOk"] = "Not içeriği güncellendi";
            return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction(nameof(Index));
        }

        public IActionResult Download(int id, string format)
        {
            var note = _context.ToplantiNotlari.Find(id);
            if (note == null) return NotFound();

            var requestedLanguage = (Request.Query["lang"].ToString() ?? string.Empty).Trim().ToLowerInvariant();
            var isEnglish = requestedLanguage == "en";

            string T(string tr, string en) => isEnglish ? en : tr;

            string HtmlSafe(string? s) => WebUtility.HtmlEncode(s ?? string.Empty);

            var baslik = note.ToplantiBasligi ?? string.Empty;
            var konum = note.Konum ?? string.Empty;
            var tarihStr = note.Tarih.ToString("dd.MM.yyyy");
            var saatStrPlain = note.Saat ?? note.Tarih.ToString("HH:mm");
            var hazirlayan = note.Hazirlayan ?? string.Empty;
            var katilimcilar = note.Katilimcilar ?? string.Empty;
            var tur = note.Tur ?? string.Empty;
            var aksiyonSahibi = note.AksiyonSahibi ?? string.Empty;
            var hedefTarihi = note.HedefTarihi ?? string.Empty;

            var konuPattern = new Regex(@"^\s*-?\s*(?<konu>.*?)(?:\s*\(Hedef Tarihi:\s*(?<tarih>[^\)]+)\))?\s*$", RegexOptions.Compiled);

            var konuSatirlari = (note.Konu ?? "")
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToList();

            var konuSatirDetaylari = konuSatirlari
                .Select(satir =>
                {
                    var match = konuPattern.Match(satir);
                    var konu = match.Success ? match.Groups["konu"].Value.Trim() : satir;
                    var satirHedefTarihi = match.Success ? match.Groups["tarih"].Value.Trim() : string.Empty;
                    return new { Konu = konu, HedefTarihi = satirHedefTarihi };
                })
                .ToList();

            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine($"<html lang=\"{(isEnglish ? "en" : "tr")}\">");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset=\"utf-8\" />");
            html.AppendLine($"    <title>{T("Toplantı Tutanağı", "Meeting Minutes")}</title>");
            html.AppendLine("    <style>");
            html.AppendLine("        body { font-family: Arial, Helvetica, sans-serif; font-size: 12px; }");
            html.AppendLine("        h1, h2 { text-align: center; margin: 4px 0; }");
            html.AppendLine("        table { border-collapse: collapse; width: 100%; }");
            html.AppendLine("        td, th { border: 1px solid #000; padding: 4px; }");
            html.AppendLine("        .header-table td { border: none; }");
            html.AppendLine("        .meta-label { background-color: #f2f2f2; font-weight: bold; width: 20%; }");
            html.AppendLine("        .meta-value { width: 80%; }");
            html.AppendLine("        .topic-header { background-color: #0070C0; color: #fff; font-weight: bold; }");
            html.AppendLine("        .topic-row { background-color: #E6F2FF; }");
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine($"    <h1>{T("İsim", "Name")}</h1>");
            html.AppendLine($"    <h2>{T("Toplantı Tutanağı", "Meeting Minutes")}</h2>");
            html.AppendLine("    <br />");
            html.AppendLine("    <table class=\"header-table\">");
            html.AppendLine("        <tr>");
            html.AppendLine("            <td style=\"width:25%; text-align:center; vertical-align:middle;\"><strong>HALKÖDE</strong></td>");
            html.AppendLine("            <td></td>");
            html.AppendLine("        </tr>");
            html.AppendLine($"        <tr><td class=\"meta-label\">{T("Toplantı Başlığı", "Meeting Title")}</td><td class=\"meta-value\">{HtmlSafe(baslik)}</td></tr>");
            html.AppendLine($"        <tr><td class=\"meta-label\">{T("Konum", "Location")}</td><td class=\"meta-value\">{HtmlSafe(konum)}</td></tr>");
            html.AppendLine("        <tr>");
            html.AppendLine($"            <td class=\"meta-label\">{T("Tarih", "Date")}</td>");
            html.AppendLine($"            <td class=\"meta-value\">{HtmlSafe(tarihStr)}&nbsp;&nbsp;&nbsp; {T("Saat", "Time")}: {HtmlSafe(saatStrPlain)}</td>");
            html.AppendLine("        </tr>");
            html.AppendLine($"        <tr><td class=\"meta-label\">{T("Hazırlayan", "Prepared By")}</td><td class=\"meta-value\">{HtmlSafe(hazirlayan)}</td></tr>");
            html.AppendLine($"        <tr><td class=\"meta-label\">{T("Katılımcılar", "Participants")}</td><td class=\"meta-value\">{HtmlSafe(katilimcilar)}</td></tr>");
            html.AppendLine("    </table>");
            html.AppendLine("    <br />");
            html.AppendLine("    <table>");
            html.AppendLine("        <tr class=\"topic-header\">");
            html.AppendLine("            <th style=\"width:5%;\">#</th>");
            html.AppendLine($"            <th style=\"width:55%;\">{T("Konu", "Topic")}</th>");
            html.AppendLine($"            <th style=\"width:15%;\">{T("Türü", "Type")}</th>");
            html.AppendLine($"            <th style=\"width:15%;\">{T("Aksiyon Sahibi", "Action Owner")}</th>");
            html.AppendLine($"            <th style=\"width:10%;\">{T("Hedef Tarihi", "Target Date")}</th>");
            html.AppendLine("        </tr>");

            if (konuSatirlari.Count == 0)
            {
                html.AppendLine("        <tr class=\"topic-row\">");
                html.AppendLine("            <td>1.</td>");
                html.AppendLine($"            <td>{HtmlSafe(note.NotIcerigi)}</td>");
                html.AppendLine($"            <td>{HtmlSafe(tur)}</td>");
                html.AppendLine($"            <td>{HtmlSafe(aksiyonSahibi)}</td>");
                html.AppendLine($"            <td>{HtmlSafe(hedefTarihi)}</td>");
                html.AppendLine("        </tr>");
            }
            else
            {
                int maddeNo = 1;
                foreach (var satir in konuSatirlari)
                {
                    var detay = konuSatirDetaylari[maddeNo - 1];
                    html.AppendLine("        <tr class=\"topic-row\">");
                    html.AppendLine($"            <td>{maddeNo}.</td>");
                    html.AppendLine($"            <td>{HtmlSafe(detay.Konu)}</td>");
                    html.AppendLine($"            <td>{HtmlSafe(tur)}</td>");
                    html.AppendLine($"            <td>{HtmlSafe(aksiyonSahibi)}</td>");
                    html.AppendLine($"            <td>{HtmlSafe(string.IsNullOrWhiteSpace(detay.HedefTarihi) ? hedefTarihi : detay.HedefTarihi)}</td>");
                    html.AppendLine("        </tr>");
                    maddeNo++;
                }
            }

            html.AppendLine("    </table>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            var safeName = string.IsNullOrWhiteSpace(note.ToplantiBasligi)
                ? "Toplanti"
                : new string(note.ToplantiBasligi.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch).ToArray());

            format = (format ?? string.Empty).Trim().ToLowerInvariant();

            if (format == "html")
            {
                var fileNameHtml = $"{(isEnglish ? "MeetingMinutes" : "ToplantiTutanagi")}_{note.Id}_{safeName}.html";
                return File(Encoding.UTF8.GetBytes(html.ToString()), "text/html; charset=utf-8", fileNameHtml);
            }

            if (format == "pdf")
            {
                using var stream = new MemoryStream();
                var doc = new Document(PageSize.A4, 36, 36, 36, 36);
                PdfWriter.GetInstance(doc, stream);
                doc.Open();

                var baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, "Cp1254", BaseFont.NOT_EMBEDDED);
                var titleFont = new Font(baseFont, 14, Font.BOLD);
                var normalFont = new Font(baseFont, 10, Font.NORMAL);

                var p1 = new Paragraph(T("İsim", "Name"), titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 4f
                };
                var p2 = new Paragraph(T("Toplantı Tutanağı", "Meeting Minutes"), titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 12f
                };
                doc.Add(p1);
                doc.Add(p2);

                var metaTable = new PdfPTable(2) { WidthPercentage = 100 };
                metaTable.SetWidths(new float[] { 1f, 3f });

                PdfPCell MetaCell(string text, bool header)
                {
                    var cell = new PdfPCell(new Phrase(text, normalFont));
                    if (header)
                    {
                        cell.BackgroundColor = new BaseColor(242, 242, 242);
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    }
                    return cell;
                }

                metaTable.AddCell(MetaCell(T("Toplantı Başlığı", "Meeting Title"), true));
                metaTable.AddCell(MetaCell(note.ToplantiBasligi ?? string.Empty, false));
                metaTable.AddCell(MetaCell(T("Konum", "Location"), true));
                metaTable.AddCell(MetaCell(note.Konum ?? string.Empty, false));
                metaTable.AddCell(MetaCell(T("Tarih", "Date"), true));
                metaTable.AddCell(MetaCell($"{tarihStr}  {T("Saat", "Time")}: {saatStrPlain}", false));
                metaTable.AddCell(MetaCell(T("Hazırlayan", "Prepared By"), true));
                metaTable.AddCell(MetaCell(note.Hazirlayan ?? string.Empty, false));
                metaTable.AddCell(MetaCell(T("Katılımcılar", "Participants"), true));
                metaTable.AddCell(MetaCell(note.Katilimcilar ?? string.Empty, false));

                doc.Add(metaTable);
                doc.Add(new Paragraph("\n"));

              var topicTable = new PdfPTable(5) { WidthPercentage = 100 };
                topicTable.SetWidths(new float[] { 0.6f, 3.1f, 1.3f, 1.5f, 1.2f });

                PdfPCell HeaderCell(string text)
                {
                    return new PdfPCell(new Phrase(text, new Font(normalFont.BaseFont, 10, Font.BOLD, BaseColor.WHITE)))
                    {
                        BackgroundColor = new BaseColor(0, 112, 192),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE
                    };
                }

                topicTable.AddCell(HeaderCell("#"));
             topicTable.AddCell(HeaderCell(T("Konu", "Topic")));
                topicTable.AddCell(HeaderCell(T("Türü", "Type")));
                topicTable.AddCell(HeaderCell(T("Aksiyon Sahibi", "Action Owner")));
                topicTable.AddCell(HeaderCell(T("Hedef Tarihi", "Target Date")));

                var topicRowColor = new BaseColor(230, 242, 255);

				if (konuSatirlari.Count == 0)
				{
					topicTable.AddCell(new PdfPCell(new Phrase("1.", normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = topicRowColor });
					topicTable.AddCell(new PdfPCell(new Phrase(note.NotIcerigi ?? string.Empty, normalFont)) { BackgroundColor = topicRowColor });
					topicTable.AddCell(new PdfPCell(new Phrase(tur, normalFont)) { BackgroundColor = topicRowColor });
					topicTable.AddCell(new PdfPCell(new Phrase(aksiyonSahibi, normalFont)) { BackgroundColor = topicRowColor });
                   topicTable.AddCell(new PdfPCell(new Phrase(hedefTarihi, normalFont)) { BackgroundColor = topicRowColor });
				}
				else
				{
					int no = 1;
                    foreach (var detay in konuSatirDetaylari)
					{
						topicTable.AddCell(new PdfPCell(new Phrase(no.ToString() + ".", normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = topicRowColor });
                        topicTable.AddCell(new PdfPCell(new Phrase(detay.Konu, normalFont)) { BackgroundColor = topicRowColor });
						topicTable.AddCell(new PdfPCell(new Phrase(tur, normalFont)) { BackgroundColor = topicRowColor });
						topicTable.AddCell(new PdfPCell(new Phrase(aksiyonSahibi, normalFont)) { BackgroundColor = topicRowColor });
                       topicTable.AddCell(new PdfPCell(new Phrase(string.IsNullOrWhiteSpace(detay.HedefTarihi) ? hedefTarihi : detay.HedefTarihi, normalFont)) { BackgroundColor = topicRowColor });
						no++;
					}
				}

                doc.Add(topicTable);
                doc.Close();

                var fileNamePdf = $"{(isEnglish ? "MeetingMinutes" : "ToplantiTutanagi")}_{note.Id}_{safeName}.pdf";
                return File(stream.ToArray(), "application/pdf", fileNamePdf);
            }

            if (format == "doc")
            {
                var fileNameDoc = $"{(isEnglish ? "MeetingMinutes" : "ToplantiTutanagi")}_{note.Id}_{safeName}.doc";
                var bytesDoc = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(html.ToString())).ToArray();
                return File(bytesDoc, "application/msword", fileNameDoc);
            }

            if (format == "txt")
            {
                var content = new StringBuilder();
                content.AppendLine($"{T("Toplantı Başlığı", "Meeting Title")}: {note.ToplantiBasligi}");
                content.AppendLine($"{T("Konum", "Location")}: {note.Konum}");
                content.AppendLine($"{T("Tarih", "Date")}: {note.Tarih:dd.MM.yyyy} {T("Saat", "Time")}: {note.Saat}");
                content.AppendLine($"{T("Hazırlayan", "Prepared By")}: {note.Hazirlayan}");
                content.AppendLine($"{T("Katılımcılar", "Participants")}: {note.Katilimcilar}");
                content.AppendLine();
                content.AppendLine($"{T("Konu", "Topic")}: ");
                if (konuSatirlari.Count == 0)
                    content.AppendLine(note.NotIcerigi ?? string.Empty);
                else
                    foreach (var sat in konuSatirlari) content.AppendLine($"- {sat}");

                var fileNameTxt = $"{(isEnglish ? "MeetingNote" : "ToplantiNotu")}_{note.Id}_{safeName}.txt";
                return File(Encoding.UTF8.GetBytes(content.ToString()), "text/plain", fileNameTxt);
            }

            if (format == "docx")
            {
                var content = html.ToString();
                var fileNameDocx = $"{(isEnglish ? "MeetingNote" : "ToplantiNotu")}_{note.Id}_{safeName}.docx";
                var bytes = Encoding.UTF8.GetBytes(content);
                return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileNameDocx);
            }

            return BadRequest();
        }
    }
}