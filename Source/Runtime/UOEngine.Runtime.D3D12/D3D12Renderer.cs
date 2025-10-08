using Vortice.Direct3D12;
using Vortice.Direct3D12.Debug;
using static Vortice.Direct3D12.D3D12;

namespace UOEngine.Runtime.D3D12;

internal class D3D12Renderer
{
    private readonly D3D12Device _device = new();
    private readonly D3D12Swapchain _viewport = new();

    public D3D12Renderer()
    {
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
            dredSettings!.SetPageFaultEnablement(DredEnablement.ForcedOn);
            dredSettings!.SetBreadcrumbContextEnablement(DredEnablement.ForcedOn);

            dredSettings.Dispose();
        }

        _device.Startup(useValidation);

        _viewport.Startup(_device);
    }
}
