#if ANDROID
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Pdf;
using Android.Print;
using Android.Print.Pdf;
using Android.Text;
using Android.Webkit;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Skia.Helpers;
using SkiaSharp;
using Paint = Android.Graphics.Paint;

namespace Avae.Printables
{
    public class PrintingService : IPrintingService
    {
        Activity activity;
        Context context;
        public PrintingService(Activity activity, Context context)
        {
            this.activity = activity;
            this.context = context;
        }

        public delegate Task PrintDelegate(string title, string file);

        private Dictionary<string, PrintDelegate> _entries = new Dictionary<string, PrintDelegate>()
        {
            { ".pdf", PrintPdf },
            {    ".jpeg" ,PrintImage },
             {   ".bmp" , PrintImage },
              {  ".jpg" , PrintImage },
               { ".png" , PrintImage },
                {".ico" , PrintImage },
                {".gif" , PrintImage },
                {".htm" , PrintHtml },
                {".html" , PrintHtml },
        };
        public Dictionary<string, PrintDelegate> Entries
        {
            get
            {
                return _entries;
            }

        }

        private static Task PrintImage(string title, string file)
        {
            return ((PrintingService)Printable.Default).PrintAsync(BitmapToPdf(file), null, title);
        }

        private static async Task PrintTxt(string title, string file)
        {
            const float pageWidth = 612f;  // Letter portrait
            const float pageHeight = 792f;
            var text = File.ReadAllText(file);
            text = text.Replace("\r\n", "\n");
            var CharactersPerPage = TxtHelper.PaginateTextByParagraphs(text, (float)pageWidth, (float)pageHeight);
            var visuals = new List<Visual>();
            foreach (var s in CharactersPerPage)
            {
                var textBlock = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 12,
                    Width = pageWidth,
                    Height = pageHeight,
                    Text = s
                };

                visuals.Add(textBlock);
            }
            await ((PrintingService)Printable.Default).PrintAsync(visuals, title);
        }

        public static Task PrintHtml(string title, string file)
        {
            var activity = ((PrintingService)Printable.Default).activity;
            var context = ((PrintingService)Printable.Default).context;

            var webView = new WebView(activity);
            webView.LoadDataWithBaseURL(null, File.ReadAllText(file), "text/html", "utf-8", null);
            var printManager = (PrintManager)context.GetSystemService(Context.PrintService);
            var printAdapter = webView.CreatePrintDocumentAdapter("MyHTMLDocument");
            printManager.Print(title, printAdapter, null);
            return Task.CompletedTask;
        }

        private static Task PrintPdf(string title, string file)
        {
            var activity = ((PrintingService)Printable.Default).activity;

            var printManager = (PrintManager)activity.GetSystemService(Context.PrintService);

            // Now we can use the preexisting print helper class
            var adapter = new PrintAdapter(file);

            printManager?.Print(title, adapter, null);

            return Task.CompletedTask;
        }

        public async Task PrintAsync(string file, Stream? stream = null, string title = "Title")
        {
            var ext = System.IO.Path.GetExtension(file).ToLower();
            if(Entries.TryGetValue(ext,out var task))
            {
                await task(title, file);
            }
            else
            {
                await PrintTxt(title, file);
            }
        }

        private static string BitmapToPdf(string file)
        {
            using var bitmap = BitmapFactory.DecodeFile(file);
            using PrintedPdfDocument pdf = new PrintedPdfDocument(((PrintingService)Printable.Default).context,
            new PrintAttributes.Builder()
                    .SetMediaSize(PrintAttributes.MediaSize.IsoA4) // A4 size
                    .SetMinMargins(new PrintAttributes.Margins(0, 0, 0, 0))
                    .Build());

            using var pageInfo = new PdfDocument.PageInfo.Builder(bitmap.Width, bitmap.Height, 1).Create();
            using var page = pdf.StartPage(pageInfo);

            // Draw the bitmap on the page
            var canvas = page.Canvas;
            var paint = new Paint(PaintFlags.FilterBitmap);
            canvas.DrawBitmap(bitmap, 0, 0, paint);

            pdf.FinishPage(page);

            var path = System.IO.Path.GetTempPath() + "test.pdf";
            using var temp = new FileStream(System.IO.Path.GetTempPath() + "test.pdf", FileMode.Create);
            pdf.WriteTo(temp);
            return path;
        }

        public async Task PrintAsync(IEnumerable<Visual> visuals, string title = "Title")
        {
            const float pageWidth = 612f;  // Letter portrait
            const float pageHeight = 792f;
            var file = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "print.pdf");

            using var doc = SKDocument.CreatePdf(file);

            foreach (var visual in visuals)
            {
                using var canvas = doc.BeginPage(pageWidth, pageHeight);
                using var image = await VisualHelper.MeasureArrange(visual, pageWidth, pageHeight, DrawingContextHelper.RenderAsync);
                canvas.DrawImage(image, 0, 0);
                doc.EndPage();
            }

            doc.Close();
            await PrintAsync(file, null, title);
        }
    }
}
#endif
