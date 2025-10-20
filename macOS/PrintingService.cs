#if MACOS
using Avalonia;
using Avalonia.Skia.Helpers;
using Moq;
using PdfKit;
using SkiaSharp;
using System.Diagnostics;

namespace Avae.Printables
{
    public class PrintingService : IPrintingService
    {
        public IEnumerable<IPrinter> GetPrinters()
        {
            var names = NSPrinter.PrinterNames;
            var printers = new List<IPrinter>();
            foreach (var name in names)
            {
                var moq = new Mock<IPrinter>();
                moq.Setup(m => m.Name).Returns(name);
                printers.Add(moq.Object);
            }

            return printers;
        }

        public Task Print(IPrinter printer, string file)
        {
            ProcessStartInfo pStartInfo = new ProcessStartInfo();
            pStartInfo.FileName = "lpr";
            pStartInfo.Arguments = $"-P \"{printer.Name}\" \"{file}\"";
            pStartInfo.UseShellExecute = false;
            pStartInfo.CreateNoWindow = true;
            Process.Start(pStartInfo);

            return Task.CompletedTask;
        }

        private Task Print(string title, NSView view, NSPrinter? printer = null)
        {
            var printInfo = new NSPrintInfo
            {
                Printer = printer,
            };

            var printOperation = NSPrintOperation.FromView(view, printInfo);            
            printOperation.ShowsPrintPanel = true;
            printOperation.ShowsProgressPanel = true;
            printOperation.RunOperation();
            view.Dispose();
            return Task.CompletedTask;
        }

        private NSView FromImage(string file)
        {
            var url = NSUrl.FromFilename(file);
            var image = new NSImage(url);
            var cell = new NSImageView()
            {
                Image = image,
                Frame = new CGRect(0, 0, image.Size.Width, image.Size.Height),
                ImageScaling = NSImageScale.ProportionallyUpOrDown
            };
            return cell;
        }

        private NSView FromPdf(string file)
        {
            return new PdfView() { Document = new PdfDocument(NSUrl.FromFilename(file)) };
        }

        public Task Print(string title, string file, Stream? stream = null)
        {
            return Print(title, Path.GetExtension(file).ToLower() switch
            {
                ".bmp" => FromImage(file),
                ".jpg" => FromImage(file),
                ".jpeg" => FromImage(file),
                ".ico" => FromImage(file),
                ".pdf" => FromPdf(file),
                _ => FromImage(file)
            });
        }

        public async Task Print(string title, IEnumerable<Visual> visuals)
        {
            float A4_WIDTH = 595.28f;
            float A4_HEIGHT = 841.89f;

            var file = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf");

            using var doc = SKDocument.CreatePdf(file);

            foreach (var visual in visuals)
            {
                using var canvas = doc.BeginPage(A4_WIDTH, A4_HEIGHT);
                using var image = await VisualHelper.MeasureArrange(visual, A4_WIDTH, A4_HEIGHT, DrawingContextHelper.RenderAsync);
                canvas.DrawImage(image, 0, 0);
                doc.EndPage();
            }

            doc.Close();

            await Print(title, file);
        }
    }
}
#endif
