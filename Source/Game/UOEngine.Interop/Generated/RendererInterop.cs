using System.Runtime.InteropServices;

namespace UOEngine.Interop
{
	public static partial class RendererInterop
	{
		[LibraryImport("UOEngine.Native.dll", StringMarshalling = StringMarshalling.Utf8)]
		public static partial UIntPtr CreateTexture(int Width, int Height, string inName);

		[LibraryImport("UOEngine.Native.dll", StringMarshalling = StringMarshalling.Utf8)]
		public static partial void SetTextureData(UIntPtr inTexture, UIntPtr inData, int inSize);

	}
}
