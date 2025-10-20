#if WINDOWS10_0_19041_0_OR_GREATER
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Moq;
using System.Diagnostics;
using System.Drawing.Printing;

namespace Avae.Printables
{
    public class PrintingService : IPrintingService
    {
        public PrintingService()
        {
             Microsoft.UI.Dispatching.DispatcherQueueController.CreateOnCurrentThread();

             Microsoft.UI.Xaml.Hosting.WindowsXamlManager.InitializeForCurrentThread();           
        }

        public IEnumerable<IPrinter> GetPrinters()
        {
            var printers = new List<IPrinter>();
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                var moq = new Mock<IPrinter>();
                moq.Setup(m => m.Name).Returns(printer);
                printers.Add(moq.Object);
            }
            return printers;
        }

        public async Task Print(IPrinter printer, string file)
        {
            var startInfo = new ProcessStartInfo(@"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe")
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };
            using (var process = Process.Start(startInfo))
            {
                if (process != null)
                {
                    process.StandardInput.WriteLine($"# Get the current default printer\r\n$defaultPrinter = (Get-WmiObject -Query \"SELECT * FROM Win32_Printer WHERE Default = TRUE\").Name\r\n\r\n# Set the desired printer as the default printer\r\n$desiredPrinter = \"Your Printer Name\"\r\n(Get-WmiObject -Query \"SELECT * FROM Win32_Printer WHERE Name = '{printer.Name}'\").SetDefaultPrinter()\r\n\r\n# Print the document\r\nStart-Process -FilePath '{file}' -Verb Print\r\n\r\n# Restore the original default printer\r\n(Get-WmiObject -Query \"SELECT * FROM Win32_Printer WHERE Name = '$defaultPrinter'\").SetDefaultPrinter()\r\n");
                    await process.WaitForExitAsync();
                }                
            }
        }

        public async Task Print(string title, string file, Stream? stream = null)
        {
            PrinterBase helper = Path.GetExtension(file).ToLower() switch
            {
                ".pdf" => new PdfPrinter(GetActiveWindow(), title, file),
                ".jpeg" => new ImagePrinter(GetActiveWindow(), title, file),
                ".bmp" => new ImagePrinter(GetActiveWindow(), title, file),
                ".jpg" => new ImagePrinter(GetActiveWindow(), title, file),
                ".ico" => new ImagePrinter(GetActiveWindow(), title, file),
                ".svg" => new SvgPrinter(GetActiveWindow(), title, file),
                _ => new TxtPrinter(GetActiveWindow(), title, file)
            };

            helper.RegisterForPrinting();
            await helper.ShowPrintUIAsync();
        }

        public async Task Print(string title, IEnumerable<Visual> visuals)
        {
            var helper = new VisualPrinter(GetActiveWindow(), title, visuals);
             helper.RegisterForPrinting();
            await helper.ShowPrintUIAsync();
        }

        private IntPtr GetActiveWindow()
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
