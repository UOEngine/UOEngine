using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D12;

namespace UOEngine.Runtime.D3D12;

internal class FrameData
{
    public ID3D12Fence Fence { get; set; } = null!;
    public ulong SubmissionFenceValue { get; set; }

    public readonly D3D12GpuDescriptorAllocator SrvGpuDescriptorAllocator;
}
