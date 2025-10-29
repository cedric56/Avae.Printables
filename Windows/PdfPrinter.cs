#if WINDOWS10_0_19041_0_OR_GREATER

using SkiaSharp;
using SkiaSharp.Views.Windows;
using System.IO;

namespace Avae.Printables
{
    public class PdfPrinter : PrinterBase
    {
        public PdfPrinter(nint handle,string title, string file)
            : base(handle, title, file)
        {
        }

        protected override Task CreatePreview(double printableWidth, double printableHeight)
        {
            var images = PDFtoImage.Conversion.ToImages(File.ReadAllBytes(file));
            foreach (var bitmap in images)
            {
                var canvas = new SKXamlCanvas
                {
                    Width = printableWidth,
                    Height = printableHeight
                };

                EventHandler<SKPaintSurfaceEventArgs>? handler = null!;
                canvas.PaintSurface += handler = (s, e) =>
                {
                    canvas.PaintSurface -= handler;
                    using var img = SKImage.FromBitmap(bitmap);
                    e.Surface.Canvas.DrawImage(img, new SKRect(0, 0, (float)printableWidth, (float)printableHeight));
                };
                canvas.Invalidate(); // still needed to trigger paint
                printPreviewPages.Add(canvas);
            }
            return Task.CompletedTask;
        }
    }
}
#endif