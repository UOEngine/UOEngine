using UOEngine.Runtime.Application;

namespace UOEngine.Tests.Present;

internal class PresentTest : UOEngineApplication
{
    public PresentTest(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}

internal class Program
{
    static int Main(string[] args)
    {
        return UOEngineAppBuilder.Configure<PresentTest>()
            .UseDefaults()
            .Start(args);
    }
}
