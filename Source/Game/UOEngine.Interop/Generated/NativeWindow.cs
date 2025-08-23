using System.Runtime.InteropServices;

namespace UOEngine.Interop
{
	public static partial class NativeWindow
	{
		[LibraryImport("UOEngine.Native.dll", StringMarshalling = StringMarshalling.Utf8)]
		public static partial IntVector2DNative GetExtents();

	}
}
