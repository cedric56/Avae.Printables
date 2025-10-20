#if IOS
using Avalonia;
using Avalonia.Skia.Helpers;
using SkiaSharp;
using System.Diagnostics;

namespace Avae.Printables
{
    public class PrintingService : IPrintingService
    {
        public IEnumerable<IPrinter> GetPrinters()
        {
            return Enumerable.Empty<IPrinter>();
        }

        public Task Print(IPrinter printer, string file)
        {
            throw new NotImplementedException();
        }

        public Task Print(string title, string file, Stream? stream = null)
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

        public async Task Print(string title, IEnumerable<Visual> visuals)
        {            
            await Print(title, await CreatePdfAsync(title, visuals));
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
