using System.Runtime.InteropServices;

namespace UOEngine.Interop
{
	public static partial class RendererInterop
	{
		[LibraryImport("UOEngine.Native.dll", StringMarshalling = StringMarshalling.Utf8)]
		public static partial UIntPtr CreateTexture(uint Width, uint Height, string inName);

		[LibraryImport("UOEngine.Native.dll", StringMarshalling = StringMarshalling.Utf8)]
		public static partial void SetTextureData(UIntPtr inTexture, UIntPtr inData, uint inSize);

	}
}
