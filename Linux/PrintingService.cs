#if GTK
using Avalonia.Skia.Helpers;
using Gtk;
using Moq;
using SkiaSharp;

namespace Avae.Printables
{
    public class PrintingService : IPrintingService
    {
        public IEnumerable<IPrinter> GetPrinters()
        {
            Gtk.Application.Init();
            var printers = new List<IPrinter>();
            
            Printer.EnumeratePrinters(new PrinterFunc(p =>
            {
                var moq = new Mock<IPrinter>();
                moq.Setup(m => m.Name).Returns(p.Name);
                printers.Add(moq.Object);
                return false;
            }), false);
            return printers;
        }

        public Task Print(IPrinter printer, string file)
        {
            return Task.CompletedTask;
        }
        
        public Task Print(string title, string file, Stream? stream = null)
        {
            Gtk.Application.Init();

            PrintOperationBase operation = Path.GetExtension(file).ToLower() switch
            {
                ".pdf" => new PdfOperation(title, file),
                ".jpeg" => new ImageOperation(title, file),
                ".bmp" => new ImageOperation(title, file),
                ".jpg" => new ImageOperation(title, file),
                ".ico" => new ImageOperation(title, file),
                ".svg" => new SvgOperation(title, file),
                _ => new TxtOperation(title, file)
            };
            operation.Run(PrintOperationAction.PrintDialog, null);
            return Task.CompletedTask;
        }

        public Task Print(string title, IEnumerable<Avalonia.Visual> visuals)
        {
            Gtk.Application.Init();
            var operation = new VisualOperation(title, visuals);
            operation.Run(PrintOperationAction.PrintDialog, null);
            return Task.CompletedTask;
        }
    }
}
#endif
