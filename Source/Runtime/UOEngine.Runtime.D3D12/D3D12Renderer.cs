using Vortice.Direct3D12;
using Vortice.Direct3D12.Debug;
using Vortice.DXGI;
using static Vortice.Direct3D12.D3D12;
using static Vortice.DXGI.DXGI;

using UOEngine.Runtime.Platform;

namespace UOEngine.Runtime.D3D12;

internal class D3D12Renderer
{
    private readonly D3D12Device _device = new();
    private readonly IWindow _window;

    private D3D12Swapchain _viewport = null!;
    private readonly D3D12CommandContext _commandContext;

    public D3D12Renderer(IWindow window)
    {
        _window = window;
    }

    public void Startup()
    {
        bool useValidation = true;

        if(useValidation && D3D12GetDebugInterface<ID3D12Debug>(out var debug).Success)
        {
            debug!.EnableDebugLayer();
            debug!.QueryInterfaceOrNull<ID3D12Debug1>()?.SetEnableGPUBasedValidation(true);
        }
        else
        {
            useValidation = false;
        }

        if (D3D12GetDebugInterface(out ID3D12DeviceRemovedExtendedDataSettings1? dredSettings).Success)
        {
            // Turn on auto-breadcrumbs and page fault reporting.
            dredSettings!.SetAutoBreadcrumbsEnablement(DredEnablement.ForcedOn);
            dredSettings.SetPageFaultEnablement(DredEnablement.ForcedOn);
            dredSettings.SetBreadcrumbContextEnablement(DredEnablement.ForcedOn);

            dredSettings.Dispose();
        }

        IDXGIFactory2 dxgiFactory2 = CreateDXGIFactory2<IDXGIFactory2>(useValidation);

        _device.Startup(dxgiFactory2, useValidation);

        _viewport = new D3D12Swapchain(_device);

        _viewport.Startup(dxgiFactory2, _window.Handle, _window.Width, _window.Height);
    }

    public void BeginFrame()
    {
        _device.BeginFrame();

        _commandContext.RenderTarget = _viewport.Backbuffer;
    }

    public void EndFrame()
    {
        _viewport.Present(_commandContext);

        _device.EndFrame();
    }
}
