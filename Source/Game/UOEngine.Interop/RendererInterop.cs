using System.Runtime.InteropServices;

namespace UOEngine.Interop
{
	public static partial class RendererInterop
	{
		[LibraryImport("UOEngine.Native.dll")]
		public static partial UIntPtr CreateTexture(int Width, int Height);

		[LibraryImport("UOEngine.Native.dll")]
		public static partial void SetTextureData(UIntPtr inTexture, UIntPtr inData, int inSize);

	}
}
