// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Rendering.Composition;
using Avalonia.VisualTree;
using System.Diagnostics;
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

    public DrawingSurfaceControl(IRenderResourceFactory resourceFactory)
    {
        _resourceFactory = resourceFactory;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Initialise();
        UpdateSurfaceVisibility(true);
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

    private void Initialise()
    {
        if(_initialised)
        {
            return;
        }

        var selfVisual = ElementComposition.GetElementVisual(this)!;
        _compositor = selfVisual.Compositor;

        Surface = _compositor.CreateDrawingSurface();

        _visual = _compositor.CreateSurfaceVisual();
        _visual.Size = new(Bounds.Width, Bounds.Height);
        _visual.Surface = Surface;

        ElementComposition.SetElementChildVisual(this, _visual);

        RecreateTexture();

        _initialised = true;
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
}

