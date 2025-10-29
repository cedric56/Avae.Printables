#if GTK
using Avalonia.X11.Interop;
using Gtk;
using SkiaSharp;
using System.Net;
using WebKit;
using Path = System.IO.Path;

namespace Avae.Printables
{  
    public class PrintingService : IPrintingService
    {
    //public delegate Task<string> ConversionDelegate(string file);
    //    public Dictionary<string, ConversionDelegate> Conversions = new Dictionary<string, ConversionDelegate>()
    //    {
    //        {    ".jpeg" , ConvertImageToPdf },
    //         {   ".bmp" , ConvertImageToPdf },
    //          {  ".jpg" , ConvertImageToPdf },
    //           { ".png" , ConvertImageToPdf },
    //            {".ico" , ConvertImageToPdf },
    //            {".gif" , ConvertImageToPdf },
    //        {".pdf" , (file) => Task.FromResult(file) },
    //    };

        //public static Task<string> ConvertImageToPdf(string file)
        //{
        //    var temp = Path.GetTempPath() + "temp.pdf";
        //    using (var doc = SKDocument.CreatePdf(temp))
        //    {
        //        using var canvas = doc.BeginPage(595, 894);
        //        using var bitmap = SKBitmap.Decode(file);
        //        using var image = SKImage.FromBitmap(bitmap);
        //        canvas.DrawImage(image, 0, 0);
        //        doc.EndPage();
        //        doc.Close();
        //    }
        //    return Task.FromResult(temp);
        //}

        public delegate Task<PrintOperationBase> PrintDelegate(string title, string file);


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

        private static Task<PrintOperationBase> PrintPdf(string title, string file)
        {
            return Task.FromResult<PrintOperationBase>(new PdfOperation(title, file));
        }

        private static Task<PrintOperationBase> PrintImage(string title, string file)
        {
            return Task.FromResult<PrintOperationBase>(new ImageOperation(title, file));
        }

        public Task<IEnumerable<PrintablePrinter>> GetPrintersAsync()
        {
            Gtk.Application.Init();
            var printers = new List<PrintablePrinter>();
            
            Printer.EnumeratePrinters(new PrinterFunc(p =>
            {
                var moq = new PrintablePrinter()
                {
                   Name= p.Name
                };
                printers.Add(moq);
                return false;
            }), false);
            return Task.FromResult(printers.AsEnumerable());
        }

        public static Task<PrintOperationBase> PrintHtml(string title, string file)
        {
            float A4_WIDTH = 595.28f;
            // Must run on GTK main thread
            _ = GtkInteropHelper.RunOnGlibThread(() =>
            {
                var view = new WebView()
                {                    
                    WidthRequest = (int)A4_WIDTH
                };
                LoadChangedHandler? handler = null!;
                view.LoadChanged += handler = (s, e) =>
                {
                    if (e.LoadEvent == LoadEvent.Finished)
                    {
                        view.LoadChanged -= handler;
                        var op = new WebKit.PrintOperation(view);
                        op.RunDialog();
                    }
                };

                view.LoadUri("file://" + file);
                return true;
            });

            return Task.FromResult<PrintOperationBase>(null!);
        }

        public async Task PrintAsync(string file, Stream? stream = null, string title = "Title")
        {
            Gtk.Application.Init();

            file = file.Replace("file://", string.Empty);
            var decodedPath = WebUtility.UrlDecode(file);
            var ext = Path.GetExtension(decodedPath).ToLower();
            
            PrintOperationBase? operation = null!;
            if (Entries.TryGetValue(ext, out var entry))
            {
                operation = await entry(title, decodedPath);
            }
            else
            {
                operation = new TxtOperation(title, decodedPath);
            }

            if (operation != null)
            {
                operation.Run(PrintOperationAction.PrintDialog, null);
            }
        }

        public Task PrintAsync(IEnumerable<Avalonia.Visual> visuals, string title = "Title")
        {
            Gtk.Application.Init();
            var operation = new VisualOperation(title, visuals);
            operation.Run(PrintOperationAction.PrintDialog, null);
            return Task.CompletedTask;
        }
    }
}
#endif
