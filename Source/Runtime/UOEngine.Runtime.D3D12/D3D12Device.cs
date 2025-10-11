using SharpGen.Runtime;
using Vortice.Direct3D;
using Vortice.Direct3D12;
using Vortice.Direct3D12.Debug;
using Vortice.DXGI;
using static Vortice.Direct3D12.D3D12;
using static Vortice.DXGI.DXGI;

namespace UOEngine.Runtime.D3D12;

internal class D3D12Device
{
    public ID3D12Device Handle { get; private set; } = null!;

    private readonly D3D12DescriptorAllocator _renderTargetViewDescriptorAllocator;
    private readonly D3D12DescriptorAllocator _srvViewDescriptorAllocator;
    private readonly D3D12DescriptorAllocator _samplerViewDescriptorAllocator;
    private readonly D3D12DescriptorAllocator _staticSamplerViewDescriptorAllocator;

    private readonly D3D12GpuDescriptorAllocator _gpuDescriptorAllocator;

    private readonly D3D12CommandQueue[] _commandQueues = [];

    public D3D12Device()
    {
        _renderTargetViewDescriptorAllocator = new D3D12DescriptorAllocator(DescriptorHeapType.RenderTargetView, 2);
        _srvViewDescriptorAllocator = new D3D12DescriptorAllocator(DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView, 16384);
        _samplerViewDescriptorAllocator = new D3D12DescriptorAllocator(DescriptorHeapType.Sampler, 8);

        _gpuDescriptorAllocator = new D3D12GpuDescriptorAllocator(this, DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView, 8);

    }

    public void Startup(IDXGIFactory2 dxgiFactory2, bool debugDevice)
    {
        ulong maxMemorySize = 0;
        IDXGIAdapter adapterToUse = null!;

        for (uint adapterIndex = 0; dxgiFactory2.EnumAdapters1(adapterIndex, out var adapter).Success; adapterIndex++)
        {
            AdapterDescription1 desc = adapter.Description1;

            // Don't select the Basic Render Driver adapter.
            if ((desc.Flags & AdapterFlags.Software) != AdapterFlags.None)
            {
                adapter.Dispose();
                continue;
            }


            if (adapter.Description.DedicatedVideoMemory > maxMemorySize)
            {
                adapterToUse = adapter;
                maxMemorySize = (ulong)adapter.Description.DedicatedVideoMemory;
            }
        }

        Handle = D3D12CreateDevice<ID3D12Device>(adapterToUse, FeatureLevel.Level_12_0);

        if (debugDevice)
        {
            var infoQueue = Handle.QueryInterfaceOrNull<ID3D12InfoQueue1>();

            if(infoQueue != null)
            {
                infoQueue.SetBreakOnSeverity(MessageSeverity.Corruption, true);
                infoQueue.SetBreakOnSeverity(MessageSeverity.Error, true);
                infoQueue.RegisterMessageCallback(DebugCallback, MessageCallbackFlags.None);
            }

        }

        _renderTargetViewDescriptorAllocator.Startup(this);
        
    }

    public D3D12CommandQueue GetQueue(CommandListType type)
    {
        return _commandQueues[(int)type];
    }

    private void DebugCallback(MessageCategory category, MessageSeverity severity, MessageId id, string description)
    {
        Console.WriteLine($"{category}: {description}");
    }
}
