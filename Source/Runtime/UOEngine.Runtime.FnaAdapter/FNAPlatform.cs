using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.Platform;

namespace Microsoft.Xna.Framework;

public partial class FNAPlatform
{
    private static UOEGameWindow _gameWindow;

    static FNAPlatform()
    {

    }

    public static IntPtr Malloc(int size)
    {
        IntPtr block;

        unsafe
        {
            void* memory = NativeMemory.Alloc((nuint)size);
            block = (nint)memory;
        }

        return block;
    }

    public static void Free(IntPtr ptr)
    {
        unsafe
        {
            NativeMemory.Free(&ptr);
        }
    }

    public delegate void SetEnvFunc(string name, string value);
    public static readonly SetEnvFunc SetEnv;

    public static GameWindow CreateWindow()
    {
        return _gameWindow;
    }

    public delegate void DisposeWindowFunc(GameWindow window);
    public static readonly DisposeWindowFunc DisposeWindow;

    public delegate void ApplyWindowChangesFunc(
        IntPtr window,
        int clientWidth,
        int clientHeight,
        bool wantsFullscreen,
        string screenDeviceName,
        ref string resultDeviceName
    );
    public static readonly ApplyWindowChangesFunc ApplyWindowChanges;

    public delegate void ScaleForWindowFunc(IntPtr window, bool invert, ref int w, ref int h);
    public static readonly ScaleForWindowFunc ScaleForWindow;

    public delegate Rectangle GetWindowBoundsFunc(IntPtr window);
    public static readonly GetWindowBoundsFunc GetWindowBounds;

    public delegate bool GetWindowResizableFunc(IntPtr window);
    public static readonly GetWindowResizableFunc GetWindowResizable;

    public delegate void SetWindowResizableFunc(IntPtr window, bool resizable);
    public static readonly SetWindowResizableFunc SetWindowResizable;

    public delegate bool GetWindowBorderlessFunc(IntPtr window);
    public static readonly GetWindowBorderlessFunc GetWindowBorderless;

    public delegate void SetWindowBorderlessFunc(IntPtr window, bool borderless);
    public static readonly SetWindowBorderlessFunc SetWindowBorderless;

    public delegate void SetWindowTitleFunc(IntPtr window, string title);
    public static readonly SetWindowTitleFunc SetWindowTitle;

    public delegate bool IsScreenKeyboardShownFunc(IntPtr window);
    public static readonly IsScreenKeyboardShownFunc IsScreenKeyboardShown;

    public delegate GraphicsAdapter RegisterGameFunc(Game game);
    public static readonly RegisterGameFunc RegisterGame;

    public delegate void UnregisterGameFunc(Game game);
    public static readonly UnregisterGameFunc UnregisterGame;

    //public delegate void PollEventsFunc(
    //    Game game,
    //    ref GraphicsAdapter currentAdapter,
    //    bool[] textInputControlDown,
    //    ref bool textInputSuppress
    //);
    //public static readonly PollEventsFunc PollEvents;

    public static void PollEvents(Game game, ref GraphicsAdapter currentAdapter, bool[] textInputControlDown, ref bool textInputSuppress)
    {

    }

    public static GraphicsAdapter[] GetGraphicsAdapters()
    {
        var testDisplayMode = new DisplayMode(1920, 1080, SurfaceFormat.Color);

        var adapter = new GraphicsAdapter(new DisplayModeCollection([testDisplayMode]), "UOEngineAdapter", "UOEngine adapter for rendering.");

        return [adapter];
    }

    public delegate DisplayMode GetCurrentDisplayModeFunc(int adapterIndex);
    public static readonly GetCurrentDisplayModeFunc GetCurrentDisplayMode;

    public delegate Keys GetKeyFromScancodeFunc(Keys scancode);
    public static readonly GetKeyFromScancodeFunc GetKeyFromScancode;

    public delegate bool IsTextInputActiveFunc(IntPtr window);
    public static readonly IsTextInputActiveFunc IsTextInputActive;

    public delegate void StartTextInputFunc(IntPtr window);
    public static readonly StartTextInputFunc StartTextInput;

    public delegate void StopTextInputFunc(IntPtr window);
    public static readonly StopTextInputFunc StopTextInput;

    public delegate void SetTextInputRectangleFunc(IntPtr window, Rectangle rectangle);
    public static readonly SetTextInputRectangleFunc SetTextInputRectangle;

    public static void GetMouseState(
        IntPtr window,
        out int x,
        out int y,
        out ButtonState left,
        out ButtonState middle,
        out ButtonState right,
        out ButtonState x1,
        out ButtonState x2
    )
    {
        x = 0;
        y = 0;
        left = ButtonState.Released;
        middle = ButtonState.Released;
        right = ButtonState.Released;
        x1 = ButtonState.Released;
        x2 = ButtonState.Released;
    }

    public delegate void SetMousePositionFunc(
        IntPtr window,
        int x,
        int y
    );
    public static readonly SetMousePositionFunc SetMousePosition;

    public delegate void OnIsMouseVisibleChangedFunc(bool visible);
    public static readonly OnIsMouseVisibleChangedFunc OnIsMouseVisibleChanged;

    public delegate bool GetRelativeMouseModeFunc(IntPtr window);
    public static readonly GetRelativeMouseModeFunc GetRelativeMouseMode;

    public delegate void SetRelativeMouseModeFunc(IntPtr window, bool enable);
    public static readonly SetRelativeMouseModeFunc SetRelativeMouseMode;

    public delegate GamePadCapabilities GetGamePadCapabilitiesFunc(int index);
    public static readonly GetGamePadCapabilitiesFunc GetGamePadCapabilities;

    public delegate GamePadState GetGamePadStateFunc(
        int index,
        GamePadDeadZone deadZoneMode
    );
    public static readonly GetGamePadStateFunc GetGamePadState;

    public delegate bool SetGamePadVibrationFunc(
        int index,
        float leftMotor,
        float rightMotor
    );
    public static readonly SetGamePadVibrationFunc SetGamePadVibration;

    public delegate bool SetGamePadTriggerVibrationFunc(
        int index,
        float leftTrigger,
        float rightTrigger
    );
    public static readonly SetGamePadTriggerVibrationFunc SetGamePadTriggerVibration;

    public delegate string GetGamePadGUIDFunc(int index);
    public static readonly GetGamePadGUIDFunc GetGamePadGUID;

    public delegate void SetGamePadLightBarFunc(int index, Color color);
    public static readonly SetGamePadLightBarFunc SetGamePadLightBar;

    public delegate bool GetGamePadGyroFunc(int index, out Vector3 gyro);
    public static readonly GetGamePadGyroFunc GetGamePadGyro;

    public delegate bool GetGamePadAccelerometerFunc(int index, out Vector3 accel);
    public static readonly GetGamePadAccelerometerFunc GetGamePadAccelerometer;

    public delegate string GetStorageRootFunc();
    public static readonly GetStorageRootFunc GetStorageRoot;

    public delegate DriveInfo GetDriveInfoFunc(string storageRoot);
    public static readonly GetDriveInfoFunc GetDriveInfo;

    public delegate IntPtr ReadFileToPointerFunc(string path, out IntPtr size);
    public static readonly ReadFileToPointerFunc ReadFileToPointer;

    public delegate void FreeFilePointerFunc(IntPtr file);
    public static readonly FreeFilePointerFunc FreeFilePointer;

    public delegate void ShowRuntimeErrorFunc(string title, string message);
    public static readonly ShowRuntimeErrorFunc ShowRuntimeError;

    public delegate Microphone[] GetMicrophonesFunc();
    public static readonly GetMicrophonesFunc GetMicrophones;

    public delegate int GetMicrophoneSamplesFunc(
        uint handle,
        byte[] buffer,
        int offset,
        int count
    );
    public static readonly GetMicrophoneSamplesFunc GetMicrophoneSamples;

    public delegate int GetMicrophoneQueuedBytesFunc(uint handle);
    public static readonly GetMicrophoneQueuedBytesFunc GetMicrophoneQueuedBytes;

    public delegate void StartMicrophoneFunc(uint handle);
    public static readonly StartMicrophoneFunc StartMicrophone;

    public delegate void StopMicrophoneFunc(uint handle);
    public static readonly StopMicrophoneFunc StopMicrophone;

    public delegate TouchPanelCapabilities GetTouchCapabilitiesFunc();
    public static readonly GetTouchCapabilitiesFunc GetTouchCapabilities;

    public delegate void UpdateTouchPanelStateFunc();
    public static readonly UpdateTouchPanelStateFunc UpdateTouchPanelState;

    public delegate int GetNumTouchFingersFunc();
    public static readonly GetNumTouchFingersFunc GetNumTouchFingers;

    public static bool SupportsOrientationChanges() => false;

    public delegate bool NeedsPlatformMainLoopFunc();
    public static readonly NeedsPlatformMainLoopFunc NeedsPlatformMainLoop;

    public delegate void RunPlatformMainLoopFunc(Game game);
    public static readonly RunPlatformMainLoopFunc RunPlatformMainLoop;

    public static readonly char[] TextInputCharacters = new char[]
    {
            (char) 2,	// Home
			(char) 3,	// End
			(char) 8,	// Backspace
			(char) 9,	// Tab
			(char) 13,	// Enter
			(char) 127,	// Delete
			(char) 22   // Ctrl+V (Paste)
    };

    public static readonly string TitleLocation;

}
