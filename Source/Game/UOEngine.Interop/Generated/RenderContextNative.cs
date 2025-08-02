using System.Runtime.InteropServices;

namespace UOEngine.Interop
{
	public static partial class RenderContextNative
	{
		[LibraryImport("UOEngine.Native.dll", StringMarshalling = StringMarshalling.Utf8)]
		public static partial void SetShaderBindingData(UIntPtr inTexture);

		[LibraryImport("UOEngine.Native.dll", StringMarshalling = StringMarshalling.Utf8)]
		public static partial UIntPtr GetShaderInstance();

		[LibraryImport("UOEngine.Native.dll", StringMarshalling = StringMarshalling.Utf8)]
		public static partial void SetBindlessTextures(UIntPtr inTextures, int inNum);

		[LibraryImport("UOEngine.Native.dll", StringMarshalling = StringMarshalling.Utf8)]
		public static partial void Draw(int inNumInstances);

	}
}
