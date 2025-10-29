#if WINDOWS10_0_19041_0_OR_GREATER
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Skia.Helpers;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System.IO;

namespace Avae.Printables
{
    public class TxtPrinter : PrinterBase
    {
        public TxtPrinter(nint handle, string title, string file)
            : base(handle, title, file)
        {
        }

        protected override Task CreatePreview(double printableWidth, double printableHeight)
        {
            var text = File.ReadAllText(file);
            text = text.Replace("\r\n", "\n");
            var CharactersPerPage = TxtHelper.PaginateTextByParagraphs(text, (float)printableWidth, (float)printableHeight);
            foreach (var s in CharactersPerPage)
            {
                var textBlock = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 12,
                    Width = printableWidth, // A4 width at 96 DPI
                    Height = printableHeight,
                    Text = s
                };

                var canvas = new SKXamlCanvas
                {
                    Width = printableWidth,
                    Height = printableHeight
                };

                EventHandler<SKPaintSurfaceEventArgs>? handler = null!;
                canvas.PaintSurface += handler = async (s, e) =>
                {
                    canvas.PaintSurface -= handler;
                    using var img = await VisualHelper.Render(textBlock, printableWidth, printableHeight, DrawingContextHelper.RenderAsync);
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