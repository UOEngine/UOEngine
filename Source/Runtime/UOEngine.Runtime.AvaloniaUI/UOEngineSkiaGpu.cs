// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Platform;
using Avalonia.Platform.Surfaces;
using Avalonia.Skia;
using Avalonia.Vulkan;
using SkiaSharp;

using UOEngine.Runtime.Core;
using UOEngine.Runtime.Renderer;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.AvaloniaUI;

internal class UOEngineSkiaGpu : ISkiaGpu
{
    public bool IsLost => false;

    public IPlatformGraphicsContext? PlatformGraphicsContext => throw new NotImplementedException();

    public GRContext GrContext { get; private set; }

    public readonly uint GraphicsQueueFamilyIndex;

    private readonly RhiInteropContext _interopContext;
    private readonly RenderSystem _renderSystem;
    private readonly RHI.IRenderer _renderer;

    public UOEngineSkiaGpu(RHI.IRenderer renderer, RenderSystem renderSystem)
    {
        _renderer = renderer;
        _renderSystem = renderSystem;

        renderer.GetInteropContext(out var interopContext);

        _interopContext = interopContext;

        GraphicsQueueFamilyIndex = interopContext.QueueFamilyIndex;

        var vkContext = new GRVkBackendContext
        {
            VkInstance = interopContext.Instance,
            VkPhysicalDevice = interopContext.PhysicalDevice,
            VkDevice = interopContext.Device,
            VkQueue = interopContext.GraphicsQueue,
            GraphicsQueueIndex = GraphicsQueueFamilyIndex,
            GetProcedureAddress = (name, instance, device) => interopContext.GetProcAddress(name, instance, device)
        };

        if (GRContext.CreateVulkan(vkContext) is not { } grContext)
        {
            throw new InvalidOperationException("Couldn't create Vulkan context");
        }

        GrContext = grContext;
    }

    public IDisposable EnsureCurrent() => EmptyDisposable.Instance;

    public ISkiaSurface? TryCreateSurface(PixelSize size, ISkiaGpuRenderSession? session) => null;

    public object? TryGetFeature(Type featureType)
    {
        return null;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public ISkiaGpuRenderTarget? TryCreateRenderTarget(IEnumerable<IPlatformRenderSurface> surfaces)
    {
        var texture = _renderSystem.UIOverlay.Texture;

        return new UOEngineSkiaRenderTarget(texture, GrContext, GraphicsQueueFamilyIndex);
    }

    public bool IsReadyToCreateRenderTarget(IEnumerable<IPlatformRenderSurface> surfaces)
    {
        return true;
    }

    public IScopedResource<GRContext> TryGetGrContext() => ScopedResource<GRContext>.Create(GrContext, EnsureCurrent().Dispose);

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
