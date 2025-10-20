#if !BROWSER
using SkiaSharp;
using System.Text;

namespace Avae.Printables
{
    public class TxtHelper
    {
        public static List<string> PaginateTextByParagraphs(
    string text,
    float pageWidth,
    float pageHeight,
    float margin = 0,
    float fontSize = 12f)
        {
            using var paint = new SKFont()
            {
                Typeface = SKTypeface.FromFamilyName("Arial"),
                Size = fontSize
            };

            float charWidth = paint.MeasureText("M"); // average character width
            float lineHeight = 7.8f;// (float)tb.DesiredSize.Height;//etrics.Descent - paint.Metrics.Ascent;

            float usableWidth = pageWidth - 2 * margin;
            float usableHeight = pageHeight - 2 * margin;

            int charsPerLine = Math.Max(1, (int)(usableWidth / charWidth));
            int linesPerPage = Math.Max(1, (int)(usableHeight / lineHeight));

            var paragraphs = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            var pages = new List<string>();
            var pageBuilder = new StringBuilder();
            int remainingLines = linesPerPage;

            foreach (var para in paragraphs)
            {
                int linesNeeded = (int)Math.Ceiling((double)para.Length / charsPerLine);
                if (linesNeeded > remainingLines && pageBuilder.Length > 0)
                {
                    pages.Add(pageBuilder.ToString().TrimEnd());
                    pageBuilder.Clear();
                    remainingLines = linesPerPage;
                }

                pageBuilder.AppendLine(para);
                //pageBuilder.AppendLine();
                remainingLines -= linesNeeded + 1;
            }

            if (pageBuilder.Length > 0)
                pages.Add(pageBuilder.ToString().TrimEnd());

            return pages;
        }
    }
}
#endif