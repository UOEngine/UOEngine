using System.Runtime.InteropServices;

namespace UOEngine.Interop
{
	public static partial class EngineInterop
	{
		[LibraryImport("UOEngine.Native.dll", StringMarshalling = StringMarshalling.Utf8)]
		public static partial int EngineInit();

		[LibraryImport("UOEngine.Native.dll", StringMarshalling = StringMarshalling.Utf8)]
		public static partial int EnginePreUpdate();

		[LibraryImport("UOEngine.Native.dll", StringMarshalling = StringMarshalling.Utf8)]
		public static partial void EnginePostUpdate();

		[LibraryImport("UOEngine.Native.dll", StringMarshalling = StringMarshalling.Utf8)]
		public static partial void EngineShutdown();

	}
}
