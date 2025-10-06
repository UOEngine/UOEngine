using Microsoft.Extensions.DependencyInjection;
using UOEngine.Plugin;

namespace UOEngine.Runtime.Renderer;

public class RendererPlugin : IPlugin
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<Renderer>();
    }

    public void Startup(IServiceProvider serviceProvider)
    {
    }
}
