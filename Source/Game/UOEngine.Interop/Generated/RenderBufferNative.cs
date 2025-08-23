using System.Runtime.InteropServices;

namespace UOEngine.Interop
{
	public static partial class RenderBufferNative
	{
		[LibraryImport("UOEngine.Native.dll", StringMarshalling = StringMarshalling.Utf8)]
		public static partial UIntPtr CreateRenderBuffer(uint inNumElements, uint inStride);

		[LibraryImport("UOEngine.Native.dll", StringMarshalling = StringMarshalling.Utf8)]
		public static partial void SetData(UIntPtr inBuffer, UIntPtr inData, uint inNumElements);

	}
}
