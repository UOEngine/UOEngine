using Avalonia.Platform;
using Avalonia.Platform.Surfaces;
using Avalonia.Vulkan;
using System;
using System.Collections.Generic;
using System.Text;

namespace UOEngine.Runtime.AvaloniaUI;

internal class UOEnginePlatformGraphicsContext : IPlatformGraphicsContext
{
    //    public IVulkanDevice Device => throw new NotImplementedException();

    //    public IVulkanInstance Instance => throw new NotImplementedException();


    //    public bool IsLost => throw new NotImplementedException();

    //    public IVulkanRenderTarget CreateRenderTarget(IEnumerable<IPlatformRenderSurface> surfaces)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void Dispose()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public IDisposable EnsureCurrent()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public object? TryGetFeature(Type featureType)
    //    {
    //        throw new NotImplementedException();
    //    }
    public bool IsLost => throw new NotImplementedException();

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public IDisposable EnsureCurrent()
    {
        throw new NotImplementedException();
    }

    public object? TryGetFeature(Type featureType)
    {
        throw new NotImplementedException();
    }
}
