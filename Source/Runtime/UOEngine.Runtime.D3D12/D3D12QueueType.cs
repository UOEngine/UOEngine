using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D12;

namespace UOEngine.Runtime.D3D12;

internal enum D3D12QueueType
{
    Direct,
    Copy,
    Compute,

    Count,

    Invalid
}

internal static class D3D12QueueTypeExtensionMethods
{
    public static CommandListType ToCommandListType(this D3D12QueueType type)
    {
        switch (type)
        {
            case D3D12QueueType.Direct: return CommandListType.Direct;
            case D3D12QueueType.Copy: return CommandListType.Copy;
            case D3D12QueueType.Compute: return CommandListType.Compute;

            default:
                throw new Exception();
        }
    }
}