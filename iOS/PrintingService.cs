#if IOS
using Avalonia;
using Avalonia.Skia.Helpers;
using SkiaSharp;
using System.Diagnostics;
using WebKit;

namespace Avae.Printables
{
    public class PrintingService : IPrintingService
    {
        public delegate Task PrintDelegate(string title, string file);

        private Dictionary<string, PrintDelegate> _entries = new Dictionary<string, PrintDelegate>()
        {
            { ".pdf", PrintDefault },
            {    ".jpeg" , PrintDefault },
             {   ".bmp" , PrintDefault },
              {  ".jpg" , PrintDefault },
               { ".png" , PrintDefault },
                {".ico" , PrintDefault },
                {".gif" , PrintDefault },
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

        public class MyDelegate : WKNavigationDelegate
        {
            private readonly TaskCompletionSource<bool> _tcs;
            public MyDelegate(TaskCompletionSource<bool> tcs)
            {
                _tcs = tcs;
            }

            public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
            {
                // HTML is fully loaded and laid out
                _tcs.TrySetResult(true);
            }

            public override void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error)
            {
                _tcs.TrySetResult(false);
            }
        }

        public static async Task PrintHtml(string title, string html)
        {
            var webView = new WKWebView(UIScreen.MainScreen.Bounds, new WKWebViewConfiguration());
            var tcs = new TaskCompletionSource<bool>();

            webView.NavigationDelegate = new MyDelegate(tcs);

            var fileUrl = NSUrl.FromFilename(html); // full path to your HTML file
            var baseDir = NSUrl.FromFilename(Path.GetDirectoryName(html)); // directory containing the file

            webView.LoadFileUrl(fileUrl, baseDir);
            // Wait for load to finish
            await tcs.Task;

            var printController = UIPrintInteractionController.SharedPrintController;
            var printInfo = UIPrintInfo.PrintInfo;
            printInfo.JobName = title;
            printInfo.OutputType = UIPrintInfoOutputType.General;

            printController.PrintInfo = printInfo;
            printController.PrintFormatter = webView.ViewPrintFormatter; // important

            // 6️⃣ Present print UI
            printController.Present(true, (controller, completed, err) =>
            {
                if (err != null)
                    Console.WriteLine($"❌ Print failed: {err.LocalizedDescription}");
                else if (completed)
                    Console.WriteLine("✅ Document printed successfully");
            });
        }

        public static Task PrintDefault(string title, string file)
        {
            var print = UIPrintInteractionController.SharedPrintController;

            if (print == null)
            {
                Console.WriteLine("Unable to print at this time.");
            }
            else
            {
                var printInfo = UIPrintInfo.PrintInfo;
                printInfo.OutputType = UIPrintInfoOutputType.General;
                printInfo.JobName = title;

                print.PrintInfo = printInfo;
                var url = NSUrl.FromFilename(file);
                Debug.WriteLine($"URL Valid: {url.CheckPromisedItemIsReachable(out _).ToString()}");
                print.PrintingItem = url;
                print.ShowsPageRange = true;
                print.Present(true, (handler, completed, error) =>
                {
                    if (!completed && error != null)
                    {
                        Console.WriteLine(error.LocalizedDescription);
                    }
                });
            }
            return Task.CompletedTask;
        }

        public async Task PrintAsync(string file, Stream? stream = null, string title = "Title")
        {
            var ext = Path.GetExtension(file).ToLower();
            if (Entries.TryGetValue(ext, out var entry))
            {
                await entry(title, file);
            }
            else
            {
                await PrintDefault(title, file);
            }
        }

        public async Task PrintAsync(IEnumerable<Visual> visuals, string title = "Title")
        {            
            await PrintAsync(await CreatePdfAsync(title, visuals), null, title);
        }

        public async Task<string> CreatePdfAsync(string title, IEnumerable<Visual> visuals)
        {
            // A4 size in points (1/72 inch)
            const float A4_WIDTH = 595.28f;
            const float A4_HEIGHT = 841.89f;

            var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");

            using var stream = File.OpenWrite(filePath);
            using var document = SKDocument.CreatePdf(stream);

            foreach (var visual in visuals)
            {
                using var page = document.BeginPage(A4_WIDTH, A4_HEIGHT);

                // Render the Avalonia visual to a Skia image
                using var image = await VisualHelper.Render(visual, A4_WIDTH, A4_HEIGHT, DrawingContextHelper.RenderAsync);

                // Draw the image on the PDF canvas
                page.DrawImage(image, new SKRect(0, 0, A4_WIDTH, A4_HEIGHT));

                document.EndPage();
            }

            document.Close();

            return filePath;
        }
    }
}
#endif
