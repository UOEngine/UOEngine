using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.Runtime.D3D12;

internal class D3D12Swapchain
{
    private D3D12Device _device = null!;

    public void Startup(D3D12Device device)
    {
        _device = device;
    }
}
