using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UOEngine.Developer.RenderDoc;

// Just added IntPtrs to pad struct for functions we do not need yet.
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct RENDERDOC_API_1_6_0
{
    public void SetCaptureFilePathTemplate(string template)
    {
        byte* utf8 = (byte*)Marshal.StringToCoTaskMemUTF8(template);

        SetCaptureFilePathTemplateDelegate(utf8);
    }

    IntPtr GetAPIVersion;

    IntPtr SetCaptureOptionU32;
    IntPtr SetCaptureOptionF32;

    IntPtr GetCaptureOptionU32;
    IntPtr GetCaptureOptionF32;

    IntPtr SetFocusToggleKeys;
    IntPtr SetCaptureKeys;

    IntPtr GetOverlayBits;
    IntPtr MaskOverlayBits;

    IntPtr RemoveHooks;

    IntPtr UnloadCrashHandler;

    //public SetCaptureFilePathTemplateDelegate SetCaptureFilePathTemplate;
    public delegate* unmanaged[Cdecl]<byte*, void> SetCaptureFilePathTemplateDelegate;

    IntPtr GetCaptureFilePathTemplate;

    IntPtr GetNumCaptures;
    IntPtr GetCapture;

    IntPtr TriggerCapture;

    IntPtr IsTargetControlConnected;

    IntPtr LaunchReplayUI;

    IntPtr SetActiveWindow;

    IntPtr StartFrameCapture;
    IntPtr IsFrameCapturing;
    IntPtr EndFrameCapture;

    IntPtr TriggerMultiFrameCapture;

    IntPtr SetCaptureFileComments;

    IntPtr DiscardFrameCapture;

    IntPtr ShowReplayUI;

    IntPtr SetCaptureTitle;
}

internal static partial class RenderDocNative
{
    [LibraryImport("renderdoc.dll")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl)})]
    public static partial int RENDERDOC_GetAPI(int version, out IntPtr api);
}
