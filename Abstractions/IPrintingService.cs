using Avalonia;

namespace Avae.Printables
{
    public interface IPrintingService
    {
        Task Print(string title, IEnumerable<Visual> visuals);
        Task Print(string title, string file, Stream? stream = null);
        Task Print(IPrinter printer, string file);
        IEnumerable<IPrinter> GetPrinters();
    }

    public interface IPrinter
    {
        string Name { get; set; }
    }
}
