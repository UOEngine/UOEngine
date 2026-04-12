// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.VisualTree;
using SkiaSharp;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.AvaloniaUI;

public class DrawingSurfaceControl : Control
{
    public bool IsSurfaceVisible { get; private set; }

    public event Action<bool>? SurfaceVisibilityChanged;
    public event Action? SurfaceRecreated;

    public RhiRenderTarget RenderTarget => _target;

    protected CompositionDrawingSurface? Surface { get; private set; }
    protected ICompositionGpuInterop? GpuInterop { get; private set; }

    private CompositionSurfaceVisual? _visual;
    private Compositor? _compositor;

    private readonly IRenderResourceFactory _resourceFactory;
    private RhiRenderTarget _target = new();
    private IRenderTexture? _texture;

    private bool _initialised = false;
    private bool _updateQueued = false;

    class TextureDrawOp : ICustomDrawOperation
    {
        public Rect Bounds { get; }

        private readonly IRenderTexture _texture;

        public TextureDrawOp(IRenderTexture texture, Rect bounds)
        {
            _texture = texture;
            Bounds = bounds;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool HitTest(Point p) => false;
        public bool Equals(ICustomDrawOperation? other) => false;

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();

            if (leaseFeature is null)
            {
                return;
            }

            using var lease = leaseFeature.Lease();
            var gr = lease.GrContext;
            var canvas = lease.SkCanvas;

            if (gr is null)
            {
                return;
            }

            _texture.GetFeature<RhiVkImageInterop>(out var vkImageInterop);

            gr.ResetContext();

            var imageInfo = new GRVkImageInfo
            {
                CurrentQueueFamily = 0,
                LevelCount = 1,
                SampleCount = 1,
                Protected = false,
                Format = vkImageInterop!.Format,
                Image = vkImageInterop.Handle,
                ImageLayout = vkImageInterop.ImageLayout,
                ImageTiling = vkImageInterop.ImageTiling,
                ImageUsageFlags = vkImageInterop.ImageUsageFlags,
                SharingMode = vkImageInterop.SharingMode
            };

            using var renderTarget = new GRBackendRenderTarget((int)_texture.Width, (int)_texture.Height, imageInfo);
            using var skSurface = SKSurface.Create(
            gr,
            renderTarget,
            GRSurfaceOrigin.TopLeft,
            SKColorType.Rgba8888,
            new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));

            if (skSurface is null)
            {
                return;
            }

            using var image = skSurface.Snapshot();

            var dest = new SKRect((float)Bounds.X,
            (float)Bounds.Y,
            (float)Bounds.Right,
            (float)Bounds.Bottom);

            canvas.DrawImage(image, dest);
        }
    }

    public DrawingSurfaceControl(IRenderResourceFactory resourceFactory)
    {
        _resourceFactory = resourceFactory;
    }

    public sealed override void Render(DrawingContext context)
    {
        context.Custom(new TextureDrawOp(_texture!, new Rect(Bounds.Size)));
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Initialise();
        UpdateSurfaceVisibility(true);
        //QueueNextFrame();
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);

        UpdateSurfaceVisibility(false);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BoundsProperty && _visual is not null)
        {
            _visual.Size = new(Bounds.Width, Bounds.Height);

            RecreateTexture();
        }
        else if (change.Property == IsVisibleProperty)
        {
            UpdateSurfaceVisibility(IsVisible);
        }
    }

    private async void Initialise()
    {
        if(_initialised)
        {
            return;
        }

        var selfVisual = ElementComposition.GetElementVisual(this)!;
        _compositor = selfVisual.Compositor;

        GpuInterop = await _compositor.TryGetCompositionGpuInterop();

        Surface = _compositor.CreateDrawingSurface();

        _visual = _compositor.CreateSurfaceVisual();
        _visual.Size = new(Bounds.Width, Bounds.Height);
        _visual.Surface = Surface;

        ElementComposition.SetElementChildVisual(this, _visual);

        RecreateTexture();

        _initialised = true;

        //QueueNextFrame();
    }

    private void RecreateTexture()
    {
        var source = this.GetPresentationSource();
        var scaling = source?.RenderScaling ?? 1.0;

        var pixelSize = PixelSize.FromSize(Bounds.Size, scaling);

        uint width = (uint)pixelSize.Width;
        uint height = (uint)pixelSize.Height;

        if(width == 0 || height == 0)
        {
            return;
        }

        _texture = _resourceFactory.CreateTexture(new RhiTextureDescription
        {
            Width = width,
            Height = height,
            Usage = RhiRenderTextureUsage.ColourTarget,
            Name = "DrawingSurfaceControlTexture"
        });

        _target.Setup(_texture);

        SurfaceRecreated?.Invoke();
    }

    private void UpdateSurfaceVisibility(bool isVisible)
    {
        if (IsSurfaceVisible == isVisible)
        {
            return;
        }

        IsSurfaceVisible = isVisible;

        SurfaceVisibilityChanged?.Invoke(isVisible);
    }

    //private void QueueNextFrame()
    //{
    //    if (_initialised && !_updateQueued && _compositor != null)
    //    {
    //        _updateQueued = true;
    //        _compositor?.RequestCompositionUpdate(Update);
    //    }
    //}

    //private void Update()
    //{
    //    _updateQueued = false;

    //    var source = this.GetPresentationSource();
    //    if (source == null)
    //        return;

    //    _visual!.Size = new(Bounds.Width, Bounds.Height);
    //    var size = PixelSize.FromSize(Bounds.Size, source.RenderScaling);

    //    Present();
    //    QueueNextFrame();
    //}

    //private void Present()
    //{
    //    if (Surface is null || GpuInterop is null || _texture is null)
    //    {
    //        return;
    //    }

    //    //var imageHandle = new PlatformHandle(_texture.Handle, KnownPlatformGraphicsExternalImageHandleTypes.VulkanOpaqueNtHandle);

    //    //var imageProperties = new PlatformGraphicsExternalImageProperties
    //    //{
    //    //    Format = PlatformGraphicsExternalImageFormat.R8G8B8A8UNorm,
    //    //    Width = (int)_texture.Width,
    //    //    Height = (int)_texture.Height,
    //    //    MemorySize = _texture.MemorySize
    //    //};

    //    var importedImage = GpuInterop.ImportImage(new UOEngineCompositionImportableSharedGpuContextImage(_texture));

    //    //var waitSemaphore = GpuInterop.ImportSemaphore(_exportedWaitSemaphore);
    //    //var signalSemaphore = GpuInterop.ImportSemaphore(_exportedSignalSemaphore);

    //    //Surface.UpdateAsync(importedImage);

    //    //Surface.UpdateWithSemaphoresAsync(importedImage, waitSemaphore, signalSemaphore);
    //}
}

internal class UOEngineCompositionImportableSharedGpuContextImage : ICompositionImportableSharedGpuContextImage
{
    internal readonly IRenderTexture Texture; 

    internal UOEngineCompositionImportableSharedGpuContextImage(IRenderTexture texture)
    {
        Texture = texture;
    }
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
