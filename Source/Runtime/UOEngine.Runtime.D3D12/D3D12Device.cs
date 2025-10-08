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

    public void Startup(bool debugDevice)
    {
        IDXGIFactory2 dxgiFactory2 = CreateDXGIFactory2<IDXGIFactory2>(debugDevice);

        ulong maxMemorySize = 0;
        IDXGIAdapter adapterToUse = null;

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

    }

    private void DebugCallback(MessageCategory category, MessageSeverity severity, MessageId id, string description)
    {
        Console.WriteLine($"{category}: {description}");
    }
}
