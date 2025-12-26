// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;

namespace UOEngine.Runtime.AvaloniaUI;

/// <summary>
/// Window implementation for UOEngine platform.
/// </summary>
internal class UOEngineTopLevelImpl : ITopLevelImpl
{
    public double DesktopScaling => throw new NotImplementedException();

    public IPlatformHandle? Handle => throw new NotImplementedException();

    public Size ClientSize { get; private set; }


    public double RenderScaling => 1.0f;

    public IEnumerable<object> Surfaces => [GetOrCreateSurface() ];

    public Action<RawInputEventArgs>? Input { get; set; }
    public Action<Rect>? Paint { get; set; }
    public Action<Size, WindowResizeReason>? Resized { get; set; }
    public Action<double>? ScalingChanged { get; set; }
    public Action<WindowTransparencyLevel>? TransparencyLevelChanged { get; set; }

    public Compositor Compositor => UOEnginePlatform.Compositor;

    public Action? Closed { get; set; }
    public Action? LostFocus { get; set; }

    public WindowTransparencyLevel TransparencyLevel
    {
        get => _transparencyLevel;
        private set
        {
            if (_transparencyLevel.Equals(value))
                return;

            _transparencyLevel = value;
            TransparencyLevelChanged?.Invoke(value);
        }
    }

    public AcrylicPlatformCompensationLevels AcrylicCompensationLevels => throw new NotImplementedException();

    private WindowTransparencyLevel _transparencyLevel = WindowTransparencyLevel.Transparent;

    private IInputRoot? _inputRoot;
    private readonly UOEnginePlatformGraphics _platformGraphics;

    private UOEngineSkiaSurface? _surface;
    private PixelSize _renderSize;

    private UOEngineSkiaSurface GetOrCreateSurface() => _surface ??= CreateSurface();

    private UOEngineSkiaSurface CreateSurface() => _platformGraphics.GetSharedContext().CreateSurface(_renderSize);

    public UOEngineTopLevelImpl(UOEnginePlatformGraphics platformGraphics)
    {
        _platformGraphics = platformGraphics;
    }

    public IPopupImpl? CreatePopup()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        Closed?.Invoke();
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
    }

    public void SetInputRoot(IInputRoot inputRoot) => _inputRoot = inputRoot;

    public void SetTransparencyLevelHint(IReadOnlyList<WindowTransparencyLevel> transparencyLevels)
    {
        foreach (var transparencyLevel in transparencyLevels)
        {
            if (transparencyLevel == WindowTransparencyLevel.Transparent || transparencyLevel == WindowTransparencyLevel.None)
            {
                TransparencyLevel = transparencyLevel;
                return;
            }
        }
    }

    public object? TryGetFeature(Type featureType)
    {
        return null;
    }

    public void OnDraw(Rect rect) => Paint?.Invoke(rect);

    public void SetRenderSize(PixelSize renderSize)
    {
        if(_renderSize == renderSize)
        {
            return;
        }

        _renderSize = renderSize;
        _surface = CreateSurface();
    }
}
