// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using UOEngine.Runtime.Core;

namespace UOEngine.Runtime.AvaloniaUI;

internal class UOEngineTopLevelImpl : ITopLevelImpl
{
    public double DesktopScaling => throw new NotImplementedException();

    public IPlatformHandle? Handle => throw new NotImplementedException();

    public Size ClientSize => throw new NotImplementedException();

    public double RenderScaling => throw new NotImplementedException();

    public IEnumerable<object> Surfaces => throw new NotImplementedException();

    public Action<RawInputEventArgs>? Input { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Action<Rect>? Paint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Action<Size, WindowResizeReason>? Resized { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Action<double>? ScalingChanged { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Action<WindowTransparencyLevel>? TransparencyLevelChanged { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public Compositor Compositor => throw new NotImplementedException();

    public Action? Closed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Action? LostFocus { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public WindowTransparencyLevel TransparencyLevel => throw new NotImplementedException();

    public AcrylicPlatformCompensationLevels AcrylicCompensationLevels => throw new NotImplementedException();

    public IPopupImpl? CreatePopup()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Point PointToClient(PixelPoint point)
    {
        throw new NotImplementedException();
    }

    public PixelPoint PointToScreen(Point point)
    {
        throw new NotImplementedException();
    }

    public void SetCursor(ICursorImpl? cursor)
    {
        throw new NotImplementedException();
    }

    public void SetFrameThemeVariant(PlatformThemeVariant themeVariant)
    {
        throw new NotImplementedException();
    }

    public void SetInputRoot(IInputRoot inputRoot)
    {
        throw new NotImplementedException();
    }

    public void SetTransparencyLevelHint(IReadOnlyList<WindowTransparencyLevel> transparencyLevels)
    {
        throw new NotImplementedException();
    }

    public object? TryGetFeature(Type featureType)
    {
        throw new NotImplementedException();
    }
}
