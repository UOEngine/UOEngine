using Microsoft.Extensions.DependencyInjection;

using UOEngine.Plugin;

namespace UOEngine.Runtime.D3D12;

internal class D3D12Plugin: IPlugin
{
    private readonly D3D12Renderer _renderer;

    public D3D12Plugin(D3D12Renderer renderer)
    {
        _renderer = renderer;
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<D3D12Renderer>();
    }

    public void Startup()
    {
        _renderer.Startup();
    }
}
