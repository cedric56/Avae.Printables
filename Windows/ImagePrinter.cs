#if WINDOWS10_0_19041_0_OR_GREATER
using SkiaSharp;
using SkiaSharp.Views.Windows;

namespace Avae.Printables
{
    public class ImagePrinter : PrinterBase
    {
        public ImagePrinter(nint handle, string title, string file)
            : base(handle, title, file)
        {
        }

        protected override Task CreatePreview(double printableWidth, double printableHeight)
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
                using var bitmap = SKBitmap.Decode(file);
                using var img = SKImage.FromBitmap(bitmap);
                e.Surface.Canvas.DrawImage(img, new SKRect(0, 0, (float)printableWidth, (float)printableHeight));
            };
            canvas.Invalidate(); // still needed to trigger paint
            printPreviewPages.Add(canvas);
            return Task.CompletedTask;
        }
    }
}
#endif