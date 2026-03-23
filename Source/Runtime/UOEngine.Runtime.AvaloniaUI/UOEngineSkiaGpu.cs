// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Platform;
using Avalonia.Platform.Surfaces;
using Avalonia.Skia;
using Avalonia.Vulkan;
using SkiaSharp;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.AvaloniaUI;

internal class UOEngineSkiaGpu : ISkiaGpu
{
    public bool IsLost => false;

    public IPlatformGraphicsContext? PlatformGraphicsContext => throw new NotImplementedException();

    private GRContext _grContext = null!;

    private readonly RhiInteropContext _interopContext;
    private readonly IRenderResourceFactory _resourceFactory;
    private readonly RHI.IRenderer _renderer;

    public UOEngineSkiaGpu(RHI.IRenderer renderer, IRenderResourceFactory resourceFactory)
    {
        _renderer = renderer;
        _resourceFactory = resourceFactory;
        renderer.GetInteropContext(out var interopContext);

        _interopContext = interopContext;

        var vkContext = new GRVkBackendContext
        {
            VkInstance = interopContext.Instance,
            VkPhysicalDevice = interopContext.PhysicalDevice,
            VkDevice = interopContext.Device,
            VkQueue = interopContext.GraphicsQueue,
            GraphicsQueueIndex = interopContext.QueueFamilyIndex,
            GetProcedureAddress = (name, instance, device) => interopContext.GetProcAddress(name, instance, device)
        };

        if (GRContext.CreateVulkan(vkContext) is not { } grContext)
            throw new InvalidOperationException("Couldn't create Vulkan context");
        
        _grContext = grContext;
    }

    public IDisposable EnsureCurrent() => EmptyDisposable.Instance;

    //public ISkiaGpuRenderTarget? TryCreateRenderTarget(IEnumerable<object> surfaces)
    //    => surfaces.OfType<UOEngineSkiaSurface>().FirstOrDefault() is { } surface ? new UOEngineSkiaRenderTarget(surface, _grContext): null;

    public ISkiaSurface? TryCreateSurface(PixelSize size, ISkiaGpuRenderSession? session) => null;

    //public UOEngineSkiaSurface CreateSurface(PixelSize size)
    //{
    //    UOEDebug.Assert(size.Width > 0);
    //    UOEDebug.Assert(size.Height > 0);

    //    var texture = _resourceFactory.CreateTexture(new RhiTextureDescription
    //    {
    //        Width = (uint)size.Width,
    //        Height = (uint)size.Height,
    //        Name = "AvaloniaUISurface",
    //        Usage = RhiRenderTextureUsage.ColourTarget
    //    });


    //    return new UOEngineSkiaSurface(skSurface, texture);
    //}

    public object? TryGetFeature(Type featureType) => null;

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public ISkiaGpuRenderTarget? TryCreateRenderTarget(IEnumerable<IPlatformRenderSurface> surfaces)
    {
        var texture = _resourceFactory.CreateTexture(new RhiTextureDescription
        {
            Height = 1080,
            Width = 1920,
            Name = "AvaloniaUITexture",
            Usage = RhiRenderTextureUsage.ColourTarget
        });

        return new UOEngineSkiaRenderTarget(texture, _grContext, _interopContext.QueueFamilyIndex);
    }

    public bool IsReadyToCreateRenderTarget(IEnumerable<IPlatformRenderSurface> surfaces)
    {
        return true;
    }

    public IScopedResource<GRContext> TryGetGrContext() => ScopedResource<GRContext>.Create(_grContext, EnsureCurrent().Dispose);

    /// <summary>
    /// Represents a disposable that does nothing on disposal.
    /// </summary>
    private sealed class EmptyDisposable : IDisposable
    {
        public static readonly EmptyDisposable Instance = new();

        private EmptyDisposable()
        {
        }

        public void Dispose()
        {
            // no op
        }
    }
}
