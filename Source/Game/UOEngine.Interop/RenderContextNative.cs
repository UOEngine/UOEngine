using System.Runtime.InteropServices;

namespace UOEngine.Interop
{
	public static partial class RenderContextNative
	{
		[LibraryImport("UOEngine.Native.dll")]
		public static partial void SetShaderBindingData(UIntPtr inTexture);

		[LibraryImport("UOEngine.Native.dll")]
		public static partial void Draw();

	}
}
