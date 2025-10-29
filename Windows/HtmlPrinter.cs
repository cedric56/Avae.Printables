#if WINDOWS10_0_19041_0_OR_GREATER
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
using System.IO;
using Windows.Foundation;

namespace Avae.Printables
{    
    public class HtmlPrinter
    {
        private string file;

        public HtmlPrinter(string file)
        {
            this.file = file;
        }

        public async Task ShowPrintUI()
        {
            try
            {
                var webView = new WebView2();
                
                await webView.EnsureCoreWebView2Async();

                TypedEventHandler<CoreWebView2, CoreWebView2DOMContentLoadedEventArgs>? loaded = null;
                webView.CoreWebView2.DOMContentLoaded += loaded = (sender, e) =>
                {
                    webView.CoreWebView2.DOMContentLoaded -= loaded;

                    webView.CoreWebView2.ShowPrintUI(CoreWebView2PrintDialogKind.System);
                };

                var ext = Path.GetExtension(file).ToLower();
                if (ext == ".pdf")
                {
                    webView.CoreWebView2.Navigate("file:///" + file);
                }
                else
                {
                    file = "file:///" + file;
                    webView.Source = new Uri(file);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Debug.WriteLine("Ensure running in x64");
            }
        }
    }
}
#endif