using Avalonia;
using Microsoft.Extensions.DependencyInjection;

#if ANDROID
using Activity = Android.App.Activity;
using Context = Android.Content.Context;
#endif

namespace Avae.Printables
{
    public static class Extensions
    {
#if ANDROID
        public static AppBuilder UsePrinting(this AppBuilder builder, Activity activity, Context context)
#else
        public static AppBuilder UsePrinting(this AppBuilder builder)
#endif
        {
#if ANDROID
            Printable.SetDefault(new PrintingService(activity, context));
#elif BROWSER || MACOS || IOS || WINDOWS10_0_19041_0_OR_GREATER || GTK
            Printable.SetDefault(new PrintingService());
#endif

#if BROWSER

#endif

            return builder;
        }
    }

    public static class Printable
    {
        static IPrintingService defaultImplementation;

        /// <summary>
        /// Provides the default implementation for static usage of this API.
        /// </summary>
        public static IPrintingService Default =>
            defaultImplementation;

        internal static void SetDefault(IPrintingService implementation) =>
            defaultImplementation = implementation;


        public static Task Print(string title, IEnumerable<Visual> visuals)
        {
            return Default.Print(title, visuals);
        }
        public static Task Print(string title, string file, Stream stream)
        {
            return Default.Print(title, file, stream);
        }

        public static Task Print(IPrinter printer, string file)
        {
            return Default.Print(printer, file);
        }
        public static IEnumerable<IPrinter> GetPrinters()
        {
            return Default.GetPrinters();
        }
    }
}
