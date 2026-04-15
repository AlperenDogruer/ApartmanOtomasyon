using System.Data;
using System.Diagnostics;
using OfficeOpenXml;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace SiteYonetim.Core.Helpers;

public static class ExportHelper
{
    static ExportHelper()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public static void ExportToPdf(DataTable data, string[] basliklar, string raporBasligi, string dosyaYolu)
    {
        var document = new PdfDocument();
        document.Info.Title = raporBasligi;

        const double margin = 40;
        const double rowHeight = 18;
        const double headerHeight = 25;

        int colCount = basliklar.Length;
        double pageWidth = 842; // A4 landscape
        double pageHeight = 595;
        double tableWidth = pageWidth - 2 * margin;
        double colWidth = tableWidth / colCount;

        int rowIndex = 0;
        int totalRows = data.Rows.Count;

        while (rowIndex <= totalRows)
        {
            PdfPage page = document.AddPage();
            page.Width = pageWidth;
            page.Height = pageHeight;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            var titleFont = new XFont("Arial", 14, XFontStyle.Bold);
            var headerFont = new XFont("Arial", 9, XFontStyle.Bold);
            var cellFont = new XFont("Arial", 8);
            var dateFont = new XFont("Arial", 8, XFontStyle.Italic);

            double y = margin;

            // Title
            gfx.DrawString(raporBasligi, titleFont, XBrushes.DarkBlue,
                new XRect(margin, y, tableWidth, 20), XStringFormats.TopLeft);
            y += 22;

            // Date
            gfx.DrawString($"Tarih: {DateTime.Now:dd.MM.yyyy HH:mm}", dateFont, XBrushes.Gray,
                new XRect(margin, y, tableWidth, 14), XStringFormats.TopLeft);
            y += 20;

            // Header
            var headerBrush = new XSolidBrush(XColor.FromArgb(30, 58, 95));
            gfx.DrawRectangle(headerBrush, margin, y, tableWidth, headerHeight);
            for (int c = 0; c < colCount; c++)
            {
                gfx.DrawString(basliklar[c], headerFont, XBrushes.White,
                    new XRect(margin + c * colWidth + 4, y + 5, colWidth - 8, headerHeight),
                    XStringFormats.TopLeft);
            }
            y += headerHeight;

            // Rows
            bool alternate = false;
            while (rowIndex < totalRows && y + rowHeight < pageHeight - margin)
            {
                if (alternate)
                {
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(240, 244, 248)),
                        margin, y, tableWidth, rowHeight);
                }

                DataRow row = data.Rows[rowIndex];
                for (int c = 0; c < colCount; c++)
                {
                    string val = row[c]?.ToString() ?? "";
                    gfx.DrawString(val, cellFont, XBrushes.Black,
                        new XRect(margin + c * colWidth + 4, y + 3, colWidth - 8, rowHeight),
                        XStringFormats.TopLeft);
                }

                // Grid lines
                gfx.DrawLine(XPens.LightGray, margin, y + rowHeight, margin + tableWidth, y + rowHeight);

                y += rowHeight;
                rowIndex++;
                alternate = !alternate;
            }

            // Border
            gfx.DrawRectangle(XPens.DarkGray, margin, margin + 56, tableWidth, y - (margin + 56));

            if (rowIndex >= totalRows) break;
        }

        document.Save(dosyaYolu);

        try { Process.Start(new ProcessStartInfo(dosyaYolu) { UseShellExecute = true }); }
        catch { }
    }

    public static void ExportToExcel(DataTable data, string[] basliklar, string raporBasligi, string dosyaYolu)
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Rapor");

        // Title
        ws.Cells[1, 1].Value = raporBasligi;
        ws.Cells[1, 1].Style.Font.Size = 14;
        ws.Cells[1, 1].Style.Font.Bold = true;
        ws.Cells[1, 1, 1, basliklar.Length].Merge = true;

        ws.Cells[2, 1].Value = $"Tarih: {DateTime.Now:dd.MM.yyyy HH:mm}";
        ws.Cells[2, 1, 2, basliklar.Length].Merge = true;

        // Headers
        for (int c = 0; c < basliklar.Length; c++)
        {
            var cell = ws.Cells[4, c + 1];
            cell.Value = basliklar[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(30, 58, 95));
            cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
        }

        // Data
        for (int r = 0; r < data.Rows.Count; r++)
        {
            for (int c = 0; c < data.Columns.Count; c++)
            {
                var val = data.Rows[r][c];
                ws.Cells[r + 5, c + 1].Value = val;

                if (val is decimal d)
                    ws.Cells[r + 5, c + 1].Style.Numberformat.Format = "#,##0.00 ₺";
            }
        }

        ws.Cells.AutoFitColumns();

        package.SaveAs(new FileInfo(dosyaYolu));

        try { Process.Start(new ProcessStartInfo(dosyaYolu) { UseShellExecute = true }); }
        catch { }
    }
}
