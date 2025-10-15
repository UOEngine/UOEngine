using Microsoft.Extensions.DependencyInjection;

using UOEngine.Plugin;
using UOEngine.Runtime.RHI;

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
        services.AddSingleton<IRenderResourceFactory, D3D12RenderResourceFactory>();
        services.AddSingleton<D3D12Device>();
    }

    public void Startup()
    {
        _renderer.Startup();
    }

    public void Shutdown()
    {
        _renderer.Dispose();
    }
}
