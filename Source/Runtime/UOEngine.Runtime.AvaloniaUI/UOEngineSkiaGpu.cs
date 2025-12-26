// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Skia;
using SkiaSharp;
using System.Drawing;

namespace UOEngine.Runtime.AvaloniaUI;

internal class UOEngineSkiaGpu : ISkiaGpu
{
    public bool IsLost => false;

    private GRContext _grContext = null!;

    public UOEngineSkiaGpu()
    {
        //_grContext = grContext;
    }

    public IDisposable EnsureCurrent() => EmptyDisposable.Instance;

    public ISkiaGpuRenderTarget? TryCreateRenderTarget(IEnumerable<object> surfaces)
        => surfaces.OfType<UOEngineSkiaSurface>().FirstOrDefault() is { } surface ? new UOEngineSkiaRenderTarget(surface, _grContext): null;

    public ISkiaSurface? TryCreateSurface(PixelSize size, ISkiaGpuRenderSession? session)
    {
        throw new NotImplementedException();
    }

    public UOEngineSkiaSurface CreateSurface(PixelSize size)
    {
        //var grVkImageInfo = new GRVkImageInfo(); // Setup info from vulkan here.

        var imageInfo = new SKImageInfo(size.Width, size.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
        var skSurface = SKSurface.Create(imageInfo);

        //var skSurface = SKSurface.Create(
        //    _grContext,
        //    new GRBackendRenderTarget(size.Width, size.Height, imageInfo),
        //    GRSurfaceOrigin.TopLeft,
        //    SKColorType.Rgba8888,
        //    new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal)
        //);

        return new UOEngineSkiaSurface(skSurface);
    }

    public object? TryGetFeature(Type featureType) => null;

    public void Dispose()
    {
        throw new NotImplementedException();
    }

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
