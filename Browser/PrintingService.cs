#if BROWSER
using Avalonia;
using Avalonia.Skia.Helpers;
using Microsoft.AspNetCore.StaticFiles;
using SkiaSharp;
using System.Runtime.InteropServices.JavaScript;

namespace Avae.Printables
{
    public partial class PrintingService : IPrintingService
    {
        public class Response
        {
            public required string Base64 { get; set; }
            public required bool Stop { get; set; }
        }
        public delegate Task<Response> PrintDelegate(string file, Stream? stream);

        private static Dictionary<string, PrintDelegate> _entries = new Dictionary<string, PrintDelegate>()
        {
            { ".pdf", DefaultPrint },
            {    ".jpeg" ,DefaultPrint },
             {   ".bmp" , DefaultPrint },
              {  ".jpg" , DefaultPrint },
               { ".png" , DefaultPrint },
                {".ico" , DefaultPrint },
                {".gif" , DefaultPrint },              
                {".htm" , DefaultPrint },
                {".html" , DefaultPrint },
        };
        public Dictionary<string, PrintDelegate> Entries
        {
            get
            {
                return _entries;
            }
        }

        public static string GetMimeType(string fileNameOrExt)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileNameOrExt, out string? contentType))
            {
                contentType = "application/octet-stream"; // default fallback
            }
            return contentType;
        }

        [JSImport("printingInterop.print", "printing")]
        public static partial void Print(string base64, string mime, string title);

        public static async Task<Response> DefaultPrint(string file, Stream? stream)
        {
            if (stream is not null)
            {
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                return new Response() { Base64 = Convert.ToBase64String(ms.ToArray()), Stop = false };
            }
            else
            {
                byte[] bytes = await File.ReadAllBytesAsync(file);
                return new Response() { Base64 = Convert.ToBase64String(bytes), Stop = false };
            }
        }

        public static async Task Invoke(string file, Stream? stream = null, string title = "Title")
        {
            var ext = Path.GetExtension(file);
            string? base64 = null;
            string? mime = GetMimeType(ext);

            if (_entries.TryGetValue(ext, out var entry))
            {
                var tuple = await entry(file, stream);
                base64 = tuple.Base64;
                if (tuple.Stop)
                    return;
            }
            else
            {
                var tuple = await DefaultPrint(file, stream);
                base64 = tuple.Base64;
            }
            Print(base64, mime, title);
        }

        public Task PrintAsync(string file, Stream? stream = null, string title = "Title")
        {
            return Invoke(file, stream, title);
        }

        public async Task PrintAsync(IEnumerable<Visual> visuals, string title = "Title")
        {
            float A4_WIDTH = 595.28f;
            float A4_HEIGHT = 841.89f;

            using var file = new MemoryStream();

            using var doc = SKDocument.CreatePdf(file);

            foreach (var visual in visuals)
            {
                using var canvas = doc.BeginPage(A4_WIDTH, A4_HEIGHT);
                using var image = await VisualHelper.MeasureArrange(visual, A4_WIDTH, A4_HEIGHT, DrawingContextHelper.RenderAsync);
                canvas.DrawImage(image, 0, 0);
                doc.EndPage();
            }

            doc.Close();

            string base64 = Convert.ToBase64String(file.ToArray());
            Print(base64, "application/pdf", title);
        }
    }
}
#endif
