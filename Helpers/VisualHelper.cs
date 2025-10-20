using Avalonia;
using Avalonia.Layout;
using SkiaSharp;

namespace Avae.Printables
{
    public class VisualHelper
    {
        public static async Task<SKImage> Render(Visual visual, double printableWidth, double printableHeight, Func<SKCanvas, Visual, Task> renderAsync)
        {
            double width = visual.Bounds.Width;
            double height = visual.Bounds.Height;

            if ((width == 0 || height == 0) && visual is Layoutable layoutable)
            {
                layoutable.Measure(new Size(printableWidth, printableHeight));
                layoutable.Arrange(new Rect(0, 0, printableWidth, printableHeight));
                layoutable.UpdateLayout();
                width = printableWidth;
                height = printableHeight;
            }

            using var surface = SKSurface.Create(new SKImageInfo((int)width, (int)height));
            var skCanvas = surface.Canvas;
            await renderAsync(skCanvas, visual);
            return surface.Snapshot();
        }

        public static async Task<SKImage> MeasureArrange(Visual visual, double printableWidth, double printableHeight, Func<SKCanvas, Visual, Task> renderAsync)
        {
            double width = visual.Bounds.Width;
            double height = visual.Bounds.Height;
            var layoutable = visual as Layoutable;

            if (layoutable is not null)
            {
                layoutable.Measure(new Size(printableWidth, printableHeight));
                layoutable.Arrange(new Rect(0, 0, printableWidth, printableHeight));
                layoutable.UpdateLayout();
            }

            using var surface = SKSurface.Create(new SKImageInfo((int)printableWidth, (int)printableHeight));
            var skCanvas = surface.Canvas;
            await renderAsync(skCanvas, visual);

            if ((width != 0 && height != 0) && layoutable is not null)
            {
                layoutable.Measure(new Size(width, height));
                layoutable.Arrange(new Rect(0, 0, width, height));
                layoutable.UpdateLayout();
            }

            return surface.Snapshot();
        }
    }
}
