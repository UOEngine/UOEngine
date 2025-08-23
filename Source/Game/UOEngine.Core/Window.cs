using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOEngine.Interop;

namespace UOEngine.Core
{
    public class Window
    {
        public UIntPtr NativeHandle { get; private set; } = UIntPtr.Zero;

        public Vector2Int Viewport => NativeWindow.GetExtents();

        public Window()
        {

        }

    }
}
