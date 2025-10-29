#if WINDOWS10_0_19041_0_OR_GREATER
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
using System.IO;
using Windows.Foundation;

namespace Avae.Printables
{
    public class HtmlHelper
    {
        private static async Task<string> PrintPdf(WebView2 webView)
        {
            var path = Path.Combine(Path.GetTempPath(), "preview.pdf");
            var settings = webView.CoreWebView2.Environment.CreatePrintSettings();
            settings.ShouldPrintBackgrounds = true;
            settings.MarginBottom =
            settings.MarginLeft =
            settings.MarginRight =
            settings.MarginTop = 0;

            await webView.CoreWebView2.PrintToPdfAsync(path, settings);
            
            return path;
        }

        public static async Task<string> ConvertToPdf(string file)
        {
            string pdfPath = string.Empty;
            try
            {
                WebView2 w = new WebView2();
                await w.EnsureCoreWebView2Async();

                var tcs = new TaskCompletionSource();
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                cts.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);

                TypedEventHandler<CoreWebView2, CoreWebView2DOMContentLoadedEventArgs>? loaded = null;
                w.CoreWebView2.DOMContentLoaded += loaded = async (sender, e) =>
                {
                    w.CoreWebView2.DOMContentLoaded -= loaded;
                    pdfPath = await PrintPdf(w); 
                    tcs.TrySetResult();
                };

                file = "file:///" + file;
                w.Source = new Uri(file);

                await tcs.Task;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Debug.WriteLine("Ensure running in x64");
            }

            return pdfPath;
        }
    }
}
#endif