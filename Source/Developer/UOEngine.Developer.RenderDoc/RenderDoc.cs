using System.Runtime.InteropServices;

namespace UOEngine.Developer.RenderDoc;

internal class RenderDoc
{
    public static bool IsRenderDocActive => Environment.GetEnvironmentVariable("RENDERDOC_CAPTUREOPTS") != null;

    private IntPtr _renderDoc;

    private int eRENDERDOC_API_Version_1_6_0 = 10600;

    private RENDERDOC_API_1_6_0 _api;

    public bool TryLoad()
    {
        // Inject RenderDoc into the process so can take captures without going through RenderDoc GUI.
        _renderDoc = NativeLibrary.Load(@"C:\Program Files\RenderDoc\RenderDoc.dll");

        if(_renderDoc == IntPtr.Zero)
        {
            return false;
        }

        int result = RenderDocNative.RENDERDOC_GetAPI(eRENDERDOC_API_Version_1_6_0, out IntPtr apiPtr);

        if(result != 1)
        {
            return false;
        }

        _api = Marshal.PtrToStructure<RENDERDOC_API_1_6_0>(apiPtr);

        string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        string captureLocation = Path.Combine(docs, "UOEngine");

        if(Directory.Exists(captureLocation) == false)
        {
            Directory.CreateDirectory(captureLocation);
        }

        unsafe
        {
            _api.SetCaptureFilePathTemplate(@$"{captureLocation}\UOEngine");
        }

        return true;
    }
}
