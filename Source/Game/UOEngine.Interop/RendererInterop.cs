using System.Runtime.InteropServices;

namespace UOEngine.Interop
{
	public static partial class RendererInterop
	{
		[LibraryImport("UOEngine.Native.dll")]
		public static partial IntPtr CreateTexture(int Width, int Height);

	}
}
