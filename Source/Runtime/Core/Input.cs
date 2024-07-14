using Silk.NET.GLFW;
using System.Diagnostics;

namespace UOEngine.Runtime.Core
{
    public enum EInputEventType
    {
        None,
        MouseMove,
        MouseButton,
        Key,
        Quit
    }

    public class Input: ITickable
    {
        public event MouseMovedEventHandler? MouseMovedEvent;

        public delegate void MouseMovedEventHandler(object sender, MouseEventArgs e);

        public Input()
        {
        }

        public void Tick(float deltaSeconds)
        {
            Debug.Assert(initialised);

            events1.Clear();

            var glfw = Glfw.GetApi();

            glfw.PollEvents();

            unsafe
            {

                if (glfw.WindowShouldClose((WindowHandle*)_windowHandle))
                {
                    events1.Add(EInputEventType.Quit);

                }
            }
        }

        public void Initialise(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;

            var glfw = Glfw.GetApi();

            unsafe
            {
                glfw.SetKeyCallback((WindowHandle*)_windowHandle, KeyCallback);
                glfw.SetMouseButtonCallback((WindowHandle*)_windowHandle, MouseButtonCallback);
                glfw.SetCursorPosCallback((WindowHandle*)_windowHandle, CursorPosCallback);
            }

            initialised = true;
        }

        public bool HasEvent(EInputEventType eventType) => events1.Contains(eventType);

        private unsafe void KeyCallback(WindowHandle* window, Keys key, int scanCode, InputAction action, KeyModifiers mods)
        {
            events1.Add(EInputEventType.Key);
        }

        private unsafe void MouseButtonCallback(WindowHandle* window, MouseButton button, InputAction action, KeyModifiers mods)
        {
            events1.Add(EInputEventType.MouseButton);
        }

        private unsafe void CursorPosCallback(WindowHandle* window, double x, double y)
        {
            events1.Add(EInputEventType.MouseMove);

            MouseMovedEvent?.Invoke(this, new MouseEventArgs { X = x, Y = y });
        }

        private List<EInputEventType> events1 = new List<EInputEventType>();

        private IntPtr _windowHandle;

        private bool initialised = false;
    }

    public class MouseEventArgs
    {
        public double X;
        public double Y;
    }
}
