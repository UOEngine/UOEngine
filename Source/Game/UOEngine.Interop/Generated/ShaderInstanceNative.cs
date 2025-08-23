using System.Runtime.InteropServices;

namespace UOEngine.Interop
{
	public static partial class ShaderInstanceNative
	{
		[LibraryImport("UOEngine.Native.dll", StringMarshalling = StringMarshalling.Utf8)]
		public static partial void SetTexture(UIntPtr inShaderInstance, string inParameterName, UIntPtr inTexture);

		[LibraryImport("UOEngine.Native.dll", StringMarshalling = StringMarshalling.Utf8)]
		public static partial void SetBuffer(UIntPtr inShaderInstance, string inParameterName, UIntPtr inBuffer);

		[LibraryImport("UOEngine.Native.dll", StringMarshalling = StringMarshalling.Utf8)]
		public static partial void SetMatrix(UIntPtr inShaderInstance, string inParameterName, UIntPtr inMatrix);

	}
}
