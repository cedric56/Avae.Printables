#if  WINDOWS10_0_19041_0_OR_GREATER
using System.Runtime.InteropServices;
using Windows.Data.Pdf;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.Graphics.Direct2D;
using Windows.Win32.Graphics.Direct3D;
using Windows.Win32.Graphics.Direct3D11;
using Windows.Win32.Graphics.Dxgi;
using Windows.Win32.Graphics.Imaging.D2D;
using Windows.Win32.Storage.Xps.Printing;
using Windows.Win32.System.WinRT.Pdf;
using Windows.Win32.Graphics.Direct2D.Common;
using Windows.Foundation;
using System.IO;

namespace Avae.Printables
{
    /// <summary>
    /// https://github.com/mgaffigan/D2DPdfPrintSample/blob/master/PrintPdfManaged/Program.cs
    /// </summary>
    class SpoolHelper
    {
        [DllImport("msoert2.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern HRESULT WriteStreamToFile(IStream pstm,
    string lpszFile, uint dwCreationDistribution, uint dwAccess);

        [DllImport("Ole32.dll", SetLastError = true)]
        static extern HRESULT CreateStreamOnHGlobal(IntPtr hGlobal, bool fDeleteOnRelease, out IStream ppstm);

        private ID3D11Device d3dDevice;
        private IDXGIDevice dxgiDevice;
        private ID2D1Factory1 d2dFactory;
        private IWICImagingFactory2 pWic;
        private ID2D1Device d2dDevice;
        private ID2D1DeviceContext d2dContextForPrint;

        public unsafe SpoolHelper()
        {
            // d3d
            PInvoke.D3D11CreateDevice(null, D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_HARDWARE, HMODULE.Null,
                D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_BGRA_SUPPORT,
                null, 0, 7 /* D3D11_SDK_VERSION */, out d3dDevice, null, out var d3dContext)
                .ThrowOnFailure();
            dxgiDevice = (IDXGIDevice)d3dDevice;

            // d2d
            var options = new D2D1_FACTORY_OPTIONS();
#if DEBUG
            options.debugLevel = D2D1_DEBUG_LEVEL.D2D1_DEBUG_LEVEL_INFORMATION;
#endif
            PInvoke.D2D1CreateFactory(D2D1_FACTORY_TYPE.D2D1_FACTORY_TYPE_MULTI_THREADED, typeof(ID2D1Factory1).GUID, options, out var od2dFactory).ThrowOnFailure();
            d2dFactory = (ID2D1Factory1)od2dFactory;
            d2dFactory.CreateDevice(dxgiDevice, out d2dDevice);
            d2dDevice.CreateDeviceContext(D2D1_DEVICE_CONTEXT_OPTIONS.D2D1_DEVICE_CONTEXT_OPTIONS_NONE, out d2dContextForPrint);

            // wic
            pWic = (IWICImagingFactory2)Activator.CreateInstance(Type.GetTypeFromCLSID(PInvoke.CLSID_WICImagingFactory)!)!;
        }

        public const long GENERIC_WRITE = (0x40000000L);        
        public const int CREATE_ALWAYS = 2;
        public unsafe void Print(string printerName, string outputFilename, byte[] ticket, PdfDocument pdfDoc)
        {
            IStream? jobOutputStream = null;
            if (printerName.ToUpper().Contains("PDF")
                || printerName.ToUpper().Contains("XPS"))
            {
                if(string.IsNullOrWhiteSpace(outputFilename))
                    throw new ArgumentNullException(nameof(outputFilename));

                CreateStreamOnHGlobal(IntPtr.Zero, true, out jobOutputStream);
            }

             
            // Create a factory for document print job.
            var documentTargetFactory = (IPrintDocumentPackageTargetFactory)new PrintDocumentPackageTargetFactory();
            var name = Path.GetFileNameWithoutExtension(outputFilename) ?? "SilentJob";

            documentTargetFactory.CreateDocumentPackageTargetForPrintJob(
                printerName, name, jobOutputStream, 
                ArrayToIStream(ticket), 
                out var docTarget);

            // Create a new print control linked to the package target.
            d2dDevice.CreatePrintControl(pWic, docTarget, null, out var printControl);

            // Open the PDF Document
            PInvoke.PdfCreateRenderer(dxgiDevice, out var pPdfRendererNative).ThrowOnFailure();
            var renderParams = new PDF_RENDER_PARAMS();

            // Write pages
            for (uint pageIndex = 0; pageIndex < pdfDoc.PageCount; pageIndex++)
            {
                var pdfPage = pdfDoc.GetPage(pageIndex);

                d2dContextForPrint.CreateCommandList(out var commandList);
                d2dContextForPrint.SetTarget(commandList);

                d2dContextForPrint.BeginDraw();
                pPdfRendererNative.RenderPageToDeviceContext(pdfPage, d2dContextForPrint, &renderParams);
                d2dContextForPrint.EndDraw();

                commandList.Close();
                printControl.AddPage(commandList, pdfPage.Size.AsD2dSizeF(), null);
            }

            
            printControl.Close();

            if (jobOutputStream is not null)
            {
                WriteStreamToFile(jobOutputStream, outputFilename, CREATE_ALWAYS, (uint)GENERIC_WRITE);
            }

        }

        private unsafe IStream ArrayToIStream(byte[] data)
        {
            PInvoke.CreateStreamOnHGlobal((HGLOBAL)null, true, out var stm).ThrowOnFailure();
            uint sz = (uint)data.Length;
            stm.SetSize(sz);
            fixed (byte* pData = data)
            {
                uint cbWritten;
                stm.Write(pData, sz, &cbWritten).ThrowOnFailure();
                if (cbWritten != sz) throw new InvalidOperationException();
            }
            return stm;
        }
    }

    internal static class D2DExtensions
    {
        public static D2D_SIZE_F AsD2dSizeF(this Size sz)
        {
            return new()
            {
                width = (float)sz.Width,
                height = (float)sz.Height,
            };
        }
    }
}
#endif