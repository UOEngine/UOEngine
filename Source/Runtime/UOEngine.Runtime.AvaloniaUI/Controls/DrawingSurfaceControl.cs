// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Rendering.Composition;

namespace UOEngine.Runtime.AvaloniaUI;

public class DrawingSurfaceControl : Control
{
    protected CompositionDrawingSurface? Surface { get; private set; }
    protected ICompositionGpuInterop? GpuInterop { get; private set; }

    private CompositionSurfaceVisual? _visual;
    private Compositor? _compositor;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Initialise();
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        Surface?.Dispose();

        base.OnDetachedFromLogicalTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BoundsProperty && _visual is not null)
        {
            _visual.Size = new(Bounds.Width, Bounds.Height);

            RecreateTexture();
        }
    }

    private async void Initialise()
    {
        var selfVisual = ElementComposition.GetElementVisual(this)!;
        _compositor = selfVisual.Compositor;

        Surface = _compositor.CreateDrawingSurface();
        _visual = _compositor.CreateSurfaceVisual();
        _visual.Size = new(Bounds.Width, Bounds.Height);
        _visual.Surface = Surface;

        ElementComposition.SetElementChildVisual(this, _visual);

        RecreateTexture();
    }

    private void RecreateTexture()
    {

    }
}

