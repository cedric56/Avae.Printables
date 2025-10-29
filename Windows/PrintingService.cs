#if WINDOWS10_0_19041_0_OR_GREATER
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Hosting;
using SkiaSharp;
using System.Drawing.Printing;
using System.IO;
using System.Printing;
using Windows.Data.Pdf;
using Windows.Storage;

namespace Avae.Printables
{
    public class PrintingService : IPrintingService
    {
        public delegate Task<PrinterBase> PrintDelegate(string title, string file);
        public delegate Task<string> ConversionDelegate(string file);
        public Dictionary<string, ConversionDelegate> Conversions = new Dictionary<string, ConversionDelegate>()
        {
            { ".jpeg" , ImageHelper.ConvertToPdf },
            { ".bmp" , ImageHelper.ConvertToPdf },
            { ".jpg" , ImageHelper.ConvertToPdf },
            { ".png" , ImageHelper.ConvertToPdf },
            {".ico" , ImageHelper.ConvertToPdf },
            {".gif" , ImageHelper.ConvertToPdf },
            {".pdf" , (file) => Task.FromResult(file) },
            {".htm" , HtmlHelper.ConvertToPdf },
            {".html" , HtmlHelper.ConvertToPdf },
        };

        private Dictionary<string, PrintDelegate> _entries = new Dictionary<string, PrintDelegate>()
        {
            { ".pdf", PrintPdf },
            { ".jpeg" ,PrintImage },
            { ".bmp" , PrintImage },
            { ".jpg" , PrintImage },
            { ".png" , PrintImage },
            { ".ico" , PrintImage },
            { ".gif" , PrintImage },
            { ".htm" , PrintHtml },
            { ".html" , PrintHtml },
        };
        public Dictionary<string, PrintDelegate> Entries
        {
            get
            {
                return _entries;
            }
        }

        public PrintingService(bool isHybrid)
        {
            if (!isHybrid)
            {
                DispatcherQueueController.CreateOnCurrentThread();

                WindowsXamlManager.InitializeForCurrentThread();
            }
        }

        public Task<IEnumerable<PrintablePrinter>> GetPrintersAsync()
        {
            var printers = new List<PrintablePrinter>();
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                printers.Add(new PrintablePrinter()
                {
                    Name = printer
                });
            }
            return Task.FromResult(printers.AsEnumerable());
        }

        private static Task<PrinterBase> PrintPdf(string title, string file)
        {
            return Task.FromResult<PrinterBase>(new PdfPrinter(GetActiveWindow(), title, file));
        }

        private static Task<PrinterBase> PrintImage(string title, string file)
        {
            return Task.FromResult<PrinterBase>(new ImagePrinter(GetActiveWindow(), title, file));
        }

        private static async Task<PrinterBase> PrintHtml(string title, string file)
        {
            if (Printable.UseEdge)
            {
                var printer = new HtmlPrinter(file);
                await printer.ShowPrintUI();
                return null!;
            }
            return new PdfPrinter(GetActiveWindow(), title, await HtmlHelper.ConvertToPdf(file));
        }

        public async Task PrintAsync(PrintablePrinter printer, string file, string ouputfilename = "Silent job")
        {
            var ext = Path.GetExtension(file).ToLower();
            var service = (PrintingService)Printable.Default;
            if (service.Conversions.TryGetValue(ext, out var conversion))
            {
                var p = LocalPrintServer.GetDefaultPrintQueue();
                var ticket = p.UserPrintTicket.GetXmlStream().ToArray();

                var pdfFile = await StorageFile.GetFileFromPathAsync(await conversion(file));
                var pdfDoc = await PdfDocument.LoadFromFileAsync(pdfFile);

                var spool = new SpoolHelper();
                spool.Print(printer.Name, ouputfilename, ticket, pdfDoc);
            }
        }

        public async Task PrintAsync(string file, Stream? stream = null, string title = "Title")
        {
            var ext = Path.GetExtension(file).ToLower();
            PrinterBase? printer = null!;
            if (Entries.TryGetValue(ext, out var entry))
            {
                printer = await entry(title, file);
            }
            else
            {
                printer = new TxtPrinter(GetActiveWindow(), title, file);
            }

            if (printer != null)
            {
                printer.RegisterForPrinting();
                await printer.ShowPrintUIAsync();
            }
        }
        
        public async Task PrintAsync(IEnumerable<Visual> visuals, string title = "Title")
        {
            var helper = new VisualPrinter(GetActiveWindow(), title, visuals);
            helper.RegisterForPrinting();
            await helper.ShowPrintUIAsync();
        }

        public static IntPtr GetActiveWindow()
        {
            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopStyleApplicationLifetime)
            {
                return desktopStyleApplicationLifetime.MainWindow.TryGetPlatformHandle().Handle;
            }
            return IntPtr.Zero;
        }
    }
}
#endif
