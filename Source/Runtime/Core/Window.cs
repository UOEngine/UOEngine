using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.Core.Contexts;
using Silk.NET.Input;
using Silk.NET.SDL;

namespace UOEngine.Runtime.Core
{
    public class Window
    {
        public event Action<Vector2D<int>>? Resized;
        public IntPtr GetHandle() => _window!.Handle;

        public uint Width { get; private set; }
        public uint Height { get; private set; }

        public IVkSurface? GetSurface() => _window?.VkSurface;

        private IWindow? _window;

        public Window()
        {
            Width = 800;
            Height = 1024;
        }

        public void Init()
        {
            //Create a window.
            var options = WindowOptions.DefaultVulkan with
            {
                Size = new Vector2D<int>((int)Width, (int)Height),
                Title = "UOEngine",
            };

            _window = Silk.NET.Windowing.Window.Create(options);
            _window.Initialize();
            if (_window.VkSurface is null)
            {
                throw new Exception("Windowing platform doesn't support Vulkan.");
            }

            _window.FramebufferResize += (Vector2D<int> obj) =>
            {
                Resized?.Invoke(obj);
            };
        }

        private void _window_FramebufferResize(Vector2D<int> obj)
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            
        }
    }
}
