using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Size_Finder.Models;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.Xml.Linq;

namespace Size_Finder.Services
{
    public class PdfService
    {
        public byte[] GenerateSizeGuidePdf(SizeFinderModel model)
        {
            var document = new PdfDocument();
            document.Info.Title = "Men's Size Guide";
            var page = document.AddPage();
            page.Width = XUnit.FromCentimeter(21);
            page.Height = XUnit.FromCentimeter(29.7);
            var gfx = XGraphics.FromPdfPage(page);

            var fontTitle = new XFont("Arial", 22, XFontStyleEx.Bold);
            var fontHeading = new XFont("Arial", 14, XFontStyleEx.Bold);
            var fontNormal = new XFont("Arial", 11, XFontStyleEx.Regular);
            var fontSmall = new XFont("Arial", 9, XFontStyleEx.Regular);

            var whiteBrush = XBrushes.White;
            var blackBrush = XBrushes.Black;
            var redBrush = XBrushes.DarkRed;
            var grayBrush = XBrushes.Gray;
            var lightGrayBrush = new XSolidBrush(XColor.FromArgb(240, 240, 240));
            var darkBrush = new XSolidBrush(XColor.FromArgb(34, 34, 34));
            var highlightBrush = new XSolidBrush(XColor.FromArgb(255, 220, 220));
            var borderPen = new XPen(XColors.LightGray, 0.5);
            var redPen = new XPen(XColors.DarkRed, 1);

            double margin = 40;
            double y = margin;
            double pageWidth = page.Width.Point;

            // Header
            gfx.DrawRectangle(darkBrush, 0, 0, pageWidth, 70);
            gfx.DrawString("Men's Size Finder Guide", fontTitle, whiteBrush,
                new XRect(0, 15, pageWidth, 40), XStringFormats.TopCenter);
            y = 90;

            // Result
            if (!string.IsNullOrEmpty(model.RecommendedSize))
            {
                gfx.DrawRectangle(lightGrayBrush, margin, y, pageWidth - margin * 2, 80);
                gfx.DrawString("Your Recommended Size", fontHeading, blackBrush,
                    new XRect(margin + 10, y + 10, 200, 30), XStringFormats.TopLeft);
                var bigFont = new XFont("Arial", 36, XFontStyleEx.Bold);
                gfx.DrawString(model.RecommendedSize, bigFont, redBrush,
                    new XRect(pageWidth - 120, y + 10, 80, 60), XStringFormats.TopLeft);
                gfx.DrawString(model.FitNote ?? "", fontSmall, grayBrush,
                    new XRect(margin + 10, y + 45, pageWidth - margin * 2 - 100, 30),
                    XStringFormats.TopLeft);
                y += 100;
            }

            // Measurements
            gfx.DrawString("Your Measurements", fontHeading, blackBrush,
                new XRect(margin, y, 300, 25), XStringFormats.TopLeft);
            y += 28;

            var measurements = new[]
            {
                ("Chest",  model.Chest,  model.ChestInInches),
                ("Waist",  model.Waist,  model.WaistInInches),
                ("Hips",   model.Hips,   model.HipsInInches),
                ("Height", model.Height, model.Height),
            };

            foreach (var (label, original, inches) in measurements)
            {
                string display = model.Unit == "cm"
                    ? $"{original} cm ({inches:F1} in)" : $"{original} in";
                gfx.DrawString($"• {label}: {display}", fontNormal, blackBrush,
                    new XRect(margin + 10, y, 400, 20), XStringFormats.TopLeft);
                y += 22;
            }

            y += 15;

            // Size Chart
            gfx.DrawString("Men's Full Size Chart", fontHeading, blackBrush,
                new XRect(margin, y, 300, 25), XStringFormats.TopLeft);
            y += 28;

            var headers = new[] { "Size", "Chest (in)", "Waist (in)", "Hips (in)", "Chest (cm)", "Waist (cm)" };
            var rows = new[]
            {
                new[] { "XS",  "32-34","26-28","32-34","81-86",  "66-71" },
                new[] { "S",   "34-36","28-30","34-36","86-91",  "71-76" },
                new[] { "M",   "36-38","30-32","36-38","91-97",  "76-81" },
                new[] { "L",   "38-40","32-34","38-40","97-102", "81-86" },
                new[] { "XL",  "40-42","34-36","40-42","102-107","86-91" },
                new[] { "XXL", "42-44","36-38","42-44","107-112","91-97" },
                new[] { "3XL", "44+",  "38+",  "44+",  "112+",  "97+"   },
            };

            double colWidth = (pageWidth - margin * 2) / headers.Length;
            double rowHeight = 22;

            gfx.DrawRectangle(darkBrush, margin, y, pageWidth - margin * 2, rowHeight);
            for (int i = 0; i < headers.Length; i++)
                gfx.DrawString(headers[i], fontSmall, whiteBrush,
                    new XRect(margin + i * colWidth + 4, y + 5, colWidth - 8, rowHeight),
                    XStringFormats.TopLeft);
            y += rowHeight;

            for (int r = 0; r < rows.Length; r++)
            {
                bool isRecommended = rows[r][0] == model.RecommendedSize;
                var rowBrush = isRecommended ? highlightBrush
                             : (r % 2 == 0 ? XBrushes.White : lightGrayBrush);
                gfx.DrawRectangle(rowBrush, margin, y, pageWidth - margin * 2, rowHeight);
                gfx.DrawRectangle(isRecommended ? redPen : borderPen,
                    margin, y, pageWidth - margin * 2, rowHeight);
                for (int c = 0; c < rows[r].Length; c++)
                {
                    var cellBrush = (isRecommended && c == 0) ? redBrush : blackBrush;
                    gfx.DrawString(rows[r][c], fontSmall, cellBrush,
                        new XRect(margin + c * colWidth + 4, y + 5, colWidth - 8, rowHeight),
                        XStringFormats.TopLeft);
                }
                y += rowHeight;
            }

            gfx.DrawString("Generated by Your Store Size Finder | support@yourstore.com",
                fontSmall, grayBrush,
                new XRect(0, page.Height.Point - 30, pageWidth, 20),
                XStringFormats.TopCenter);

            using var stream = new MemoryStream();
            document.Save(stream, false);
            return stream.ToArray();
        }
    }
}