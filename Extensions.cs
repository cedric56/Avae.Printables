using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

#if ANDROID
using Activity = Android.App.Activity;
using Context = Android.Content.Context;
#endif

namespace Avae.Printables
{

    public static class Extensions
    {
#if ANDROID
        public static AppBuilder UsePrintables(this AppBuilder builder, Activity activity, Context context)
#elif WINDOWS10_0_19041_0_OR_GREATER
        
        public static AppBuilder UsePrintables(this AppBuilder builder, bool useEdge = false, bool isHybrid = false)
#else
        public static AppBuilder UsePrintables(this AppBuilder builder)
#endif
        {
#if ANDROID
            Printable.SetDefault(new PrintingService(activity, context));
#elif BROWSER || MACOS || IOS || GTK
            Printable.SetDefault(new PrintingService());
#elif WINDOWS10_0_19041_0_OR_GREATER
            Printable.SetDefault(new PrintingService(isHybrid));
            Printable.UseEdge = useEdge;
#endif

            return builder;
        }
    }

    /// <summary>
    /// Provides static, application-wide access to printing functionality.
    /// Wraps an <see cref="IPrintingService"/> implementation (e.g., <see cref="PrintingService"/>) 
    /// to make printing easier to call from anywhere in the app.
    /// </summary>
    public static class Printable
    {
#if WINDOWS10_0_19041_0_OR_GREATER

        /// <summary>
        /// Indicates whether the system should use the Microsoft Edge printing engine
        /// (for HTML or web-based print jobs) instead of internal PDF rendering.
        /// </summary>
        public static bool UseEdge { get; set; }
#endif

        static IPrintingService defaultImplementation;

        /// <summary>
        /// Provides the default implementation for static usage of this API.
        /// </summary>
        public static IPrintingService Default =>
            defaultImplementation;

        /// <summary>
        /// Sets the default implementation for static usage of this API.
        /// </summary>
        /// <param name="implementation"></param>
        public static void SetDefault(IPrintingService implementation) =>
            defaultImplementation = implementation;

        /// <summary>
        /// Prints a collection of UI visuals (for example, controls, pages, or custom drawings).
        /// This is typically used in Avalonia or WPF to print in-memory visual elements.
        /// </summary>
        /// <param name="visuals">The visuals to print.</param>
        /// <param name="jobTitle">The title of the print job (displayed in printer queue).</param>
        /// <returns>A task representing the asynchronous print operation.</returns>
        public static Task PrintAsync(IEnumerable<Visual> visuals, string jobTitle = "Title")
        {
            return Default.PrintAsync(visuals, jobTitle);
        }

        /// <summary>
        /// Prints a file (such as PDF, image, HTML, or text).
        /// The printing behavior depends on the file extension and available print handlers.
        /// </summary>
        /// <param name="file">The path of the file to print.</param>
        /// <param name="stream">Optional data stream for the file content.</param>
        /// <param name="jobTitle">Title shown in the print dialog or spooler.</param>
        /// <returns>A task representing the asynchronous print operation.</returns>
        public static Task PrintAsync(string file, Stream? stream = null, string jobTitle = "Title")
        {
            return Default.PrintAsync(file, stream, jobTitle);
        }

        /// <summary>
        /// Sends a file directly to a specific printer without showing a print dialog.
        /// Useful for "silent" or background printing scenarios.
        /// </summary>
        /// <param name="printer">The target printer to send the job to.</param>
        /// <param name="file">The file to print.</param>
        /// <param name="ouputfilename">The output filename.</param>
        /// <returns>A task representing the asynchronous print operation.</returns>
        public static Task PrintAsync(PrintablePrinter printer, string file, string ouputfilename = "")
        {
            return Default.PrintAsync(printer, file, ouputfilename);
        }

        /// <summary>
        /// Opens a generic print dialog that allows the user to select printer and options.
        /// Used for printing main view visual.
        /// </summary>
        /// <param name="jobTitle">The title displayed in the print dialog and queue.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static Task PrintAsync(string jobTitle = "Title")
        {
            return Default.PrintAsync(jobTitle);
        }

        /// <summary>
        /// Retrieves the list of available printers on the system.
        /// </summary>
        /// <returns>
        /// A task that, when completed, returns a collection of <see cref="PrintablePrinter"/> objects.
        /// Each represents an installed printer that can be used for printing.
        /// </returns>
        public static Task<IEnumerable<PrintablePrinter>> GetPrintersAsync()
        {
            return Default.GetPrintersAsync();
        }
    }
}
