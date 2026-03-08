using UOEngine.Runtime.Application;

namespace UOEngine.Tests.DynamicVertexBuffer;

internal class Program
{
    static int Main(string[] args)
    {
        return BuildApp().UseDefaults().Start(args);
    }

    public static UOEngineAppBuilder BuildApp() => UOEngineAppBuilder.Configure<DynamicVertexBufferTest>();
}
