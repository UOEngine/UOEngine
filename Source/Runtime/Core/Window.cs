using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.Core.Contexts;
using Silk.NET.Input;

namespace UOEngine.Runtime.Core
{
    public class Window
    {
        public Window()
        {
        }

        public void Init()
        {
            //Create a window.
            var options = WindowOptions.DefaultVulkan with
            {
                Size = new Vector2D<int>(WIDTH, HEIGHT),
                Title = "ClassicUO2",
            };

            _window = Silk.NET.Windowing.Window.Create(options);
            _window.Initialize();
            if (_window.VkSurface is null)
            {
                throw new Exception("Windowing platform doesn't support Vulkan.");
            }
        }

        public void Shutdown()
        {
            
        }

        public IntPtr GetHandle() => _window!.Handle;

        public IVkSurface? GetSurface() => _window?.VkSurface;

        private IWindow? _window;

        const int WIDTH = 800;
        const int HEIGHT = 600;

    }
}
