using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avae.Printables
{
    internal class ImageHelper
    {
        public static Task<string> ConvertToPdf(string file)
        {
            var temp = Path.GetTempPath() + "temp.pdf";
            using (var doc = SKDocument.CreatePdf(temp))
            {
                using var canvas = doc.BeginPage(595, 894);
                using var bitmap = SKBitmap.Decode(file);
                using var image = SKImage.FromBitmap(bitmap);
                canvas.DrawImage(image, 0, 0);
                doc.EndPage();
                doc.Close();
            }
            return Task.FromResult(temp);
        }
    }
}
