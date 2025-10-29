#if WINDOWS10_0_19041_0_OR_GREATER
using Avalonia;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Printing;
using Windows.Graphics.Printing;
using Windows.Storage.Streams;

namespace Avae.Printables
{
    public abstract class PrinterBase
    {
        /// <summary>
        /// PrintDocument is used to prepare the pages for printing.
        /// Prepare the pages to print in the handlers for the Paginate, GetPreviewPage, and AddPages events.
        /// </summary>
        protected PrintDocument printDocument;

        /// <summary>
        /// Marker interface for document source
        /// </summary>
        public IPrintDocumentSource printDocumentSource;

        /// <summary>
        /// A list of UIElements used to store the print preview pages.  This gives easy access
        /// to any desired preview page.
        /// </summary>
        protected List<UIElement> printPreviewPages;
        internal IEnumerable<Visual> visuals;
        protected string file;
        private string title;
        private IntPtr handle;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scenarioPage">The scenario page constructing us</param>
        public PrinterBase(nint handle, string title, IEnumerable<Visual> visuals)
        //: this()
        {
            printPreviewPages = new List<UIElement>();
            this.visuals = visuals;
            this.handle = handle;
            this.title = title;
        }

        public PrinterBase(nint handle, string title, string file)
        //: this()
        {
            printPreviewPages = new List<UIElement>();
            this.handle = handle;
            this.file = file;
            this.title = title;
        }

        /// <summary>
        /// This function registers the app for printing with Windows and sets up the necessary event handlers for the print process.
        /// </summary>
        public virtual void RegisterForPrinting()
        {
            printDocument = new PrintDocument();
            printDocumentSource = printDocument.DocumentSource;
            printDocument.Paginate += CreatePrintPreviewPages;
            printDocument.GetPreviewPage += GetPrintPreviewPage;
            printDocument.AddPages += AddPrintPages;

            PrintManager printMan = PrintManagerInterop.GetForWindow(handle);
            printMan.PrintTaskRequested -= PrintTaskRequested;
            printMan.PrintTaskRequested += PrintTaskRequested;
        }

        /// <summary>
        /// This function unregisters the app for printing with Windows.
        /// </summary>
        public virtual void UnregisterForPrinting()
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (printDocument != null)
                {
                    printDocument.Paginate -= CreatePrintPreviewPages;
                    printDocument.GetPreviewPage -= GetPrintPreviewPage;
                    printDocument.AddPages -= AddPrintPages;
                    printDocument = null!;
                }

                // Remove the handler for printing initialization.
                PrintManager printMan = PrintManagerInterop.GetForWindow(handle);
                printMan.PrintTaskRequested -= PrintTaskRequested;
            });
        }

        public async Task ShowPrintUIAsync()
        {
            await PrintManagerInterop.ShowPrintUIForWindowAsync(handle);
        }

        /// <summary>
        /// This is the event handler for PrintManager.PrintTaskRequested.
        /// </summary>
        /// <param name="sender">PrintManager</param>
        /// <param name="e">PrintTaskRequestedEventArgs </param>
        protected virtual void PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs e)
        {
            PrintTask printTask = null!;
            printTask = e.Request.CreatePrintTask(title, sourceRequested =>
            {
                // Print Task event handler is invoked when the print job is completed.
                printTask.Completed += (s, args) =>
                {
                    UnregisterForPrinting();
                };
                sourceRequested.SetSource(printDocumentSource);
            });
        }

        protected abstract Task CreatePreview(double printableWidth, double printableHeight);
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// This is the event handler for PrintDocument.Paginate. It creates print preview pages for the app.
        /// </summary>
        /// <param name="sender">PrintDocument</param>
        /// <param name="e">Paginate Event Arguments</param>
        protected virtual async void CreatePrintPreviewPages(object sender, PaginateEventArgs e)
        {
            //using (await _mutex.LockAsync())
            //{
            try
            {
                await _semaphore.WaitAsync();

                printPreviewPages.Clear();

                PrintTaskOptions printingOptions = (PrintTaskOptions)e.PrintTaskOptions;
                PrintPageDescription pageDescription = printingOptions.GetPageDescription(0);

                var imageableRect = pageDescription.ImageableRect;
                double printableWidth = imageableRect.Width;
                double printableHeight = imageableRect.Height;

                await CreatePreview(printableWidth, printableHeight);

                PrintDocument printDoc = (PrintDocument)sender;
                printDoc.SetPreviewPageCount(printPreviewPages.Count, PreviewPageCountType.Intermediate);
            }
            finally
            {
                _semaphore.Release();
            }
            //}
        }

        /// <summary>
        /// This is the event handler for PrintDocument.GetPrintPreviewPage. It provides a specific print preview page,
        /// in the form of an UIElement, to an instance of PrintDocument. PrintDocument subsequently converts the UIElement
        /// into a page that the Windows print system can deal with.
        /// </summary>
        /// <param name="sender">PrintDocument</param>
        /// <param name="e">Arguments containing the preview requested page</param>
        protected virtual async void GetPrintPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            await _semaphore.WaitAsync(); // ⏳ waits until CreatePrintPreviewPages is done
            try
            {
                PrintDocument printDoc = (PrintDocument)sender;
                printDoc.SetPreviewPage(e.PageNumber, printPreviewPages[e.PageNumber - 1]);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// This is the event handler for PrintDocument.AddPages. It provides all pages to be printed, in the form of
        /// UIElements, to an instance of PrintDocument. PrintDocument subsequently converts the UIElements
        /// into a pages that the Windows print system can deal with.
        /// </summary>
        /// <param name="sender">PrintDocument</param>
        /// <param name="e">Add page event arguments containing a print task options reference</param>
        protected virtual void AddPrintPages(object sender, AddPagesEventArgs e)
        {
            // Loop over all of the preview pages and add each one to  add each page to be printied
            for (int i = 0; i < printPreviewPages.Count; i++)
            {
                // We should have all pages ready at this point...
                printDocument.AddPage(printPreviewPages[i]);
            }

            PrintDocument printDoc = (PrintDocument)sender;

            // Indicate that all of the print pages have been provided
            printDoc.AddPagesComplete();
        }
    }
}
#endif