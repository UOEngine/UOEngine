// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Platform;
using Avalonia.Platform.Surfaces;
using Avalonia.Rendering.Composition;
using Avalonia.Vulkan;

using UOEngine.Runtime.Core;
using UOEngine.Runtime.Platform;

using UOEngineMouseButton = UOEngine.Runtime.Core.MouseButton;

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

    internal IInputRoot Owner => _owner ?? throw new InvalidOperationException($"{nameof(SetInputRoot)} must have been called");
    IPlatformRenderSurface[] ITopLevelImpl.Surfaces => [];

    private WindowTransparencyLevel _transparencyLevel = WindowTransparencyLevel.Transparent;

    private IInputRoot? _owner;
    private readonly UOEnginePlatformGraphics _platformGraphics;

    private PixelSize _renderSize;

    private readonly MouseDevice _mouse = new();

    private readonly IWindow _window;

    private Point _lastMousePosition;
    private RawInputModifiers _mouseModifiers = RawInputModifiers.None;

    public UOEngineTopLevelImpl(UOEnginePlatformGraphics platformGraphics, IWindow window, InputManager inputManager)
    {
        _platformGraphics = platformGraphics;
        _window = window;

        SetRenderSize(new PixelSize((int)_window.RenderTargetWidth, (int)_window.RenderTargetHeight));

        window.OnResized += (window) =>
        {
            SetRenderSize(new PixelSize((int)_window.RenderTargetWidth, (int)_window.RenderTargetHeight));

            Resized?.Invoke( new Size(window.RenderTargetWidth, window.RenderTargetHeight), WindowResizeReason.User);
        };

        inputManager.MouseMoved += (x, y) =>
        {
            _lastMousePosition = new Point(RenderScaling * x, RenderScaling * y);

            UpdateMouseInput(RawPointerEventType.Move);
        };

        inputManager.MouseButtonDown += button =>
        {
            RawPointerEventType eventType = button switch
            {
                UOEngineMouseButton.Invalid => throw new NotImplementedException(),
                UOEngineMouseButton.Left => RawPointerEventType.LeftButtonDown,
                UOEngineMouseButton.Middle => throw new NotImplementedException(),
                UOEngineMouseButton.Right => throw new NotImplementedException(),
                UOEngineMouseButton.Back => throw new NotImplementedException(),
                UOEngineMouseButton.Forward => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };

            _mouseModifiers |= MouseButtonToInputModifier(button);

            UpdateMouseInput(eventType);
        };

        inputManager.MouseButtonUp += button =>
        {
            RawPointerEventType eventType = button switch
            {
                UOEngineMouseButton.Invalid => throw new NotImplementedException(),
                UOEngineMouseButton.Left => RawPointerEventType.LeftButtonUp,
                UOEngineMouseButton.Middle => throw new NotImplementedException(),
                UOEngineMouseButton.Right => throw new NotImplementedException(),
                UOEngineMouseButton.Back => throw new NotImplementedException(),
                UOEngineMouseButton.Forward => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };

            _mouseModifiers &= ~MouseButtonToInputModifier(button);

            UpdateMouseInput(eventType);
        };
    }

    public IPopupImpl? CreatePopup()
    {
        return null;
    }

    public void Dispose()
    {
        Closed?.Invoke();
    }

    public Point PointToClient(PixelPoint point)
    {
        return new Point(point.X, point.Y) / RenderScaling;
    }

    public PixelPoint PointToScreen(Point point)
    {
        point *= RenderScaling;

        //_window.
        //var p = new POINT { X = (int)point.X, Y = (int)point.Y };
        //ClientToScreen(_hwnd, ref p);

        return new PixelPoint((int)point.X, (int)point.Y);
    }

    public void SetCursor(ICursorImpl? cursor)
    {
        //throw new NotImplementedException();
    }

    public void SetFrameThemeVariant(PlatformThemeVariant themeVariant)
    {
    }

    public void SetInputRoot(IInputRoot inputRoot) => _owner = inputRoot;

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

        const float renderScale = 1.0f;

        ClientSize = renderSize.ToSize(renderScale);

        _renderSize = renderSize;
    }

    private static RawInputModifiers MouseButtonToInputModifier(UOEngineMouseButton button) => button switch
    {
        UOEngineMouseButton.Invalid => throw new NotImplementedException(),
        UOEngineMouseButton.Left => RawInputModifiers.LeftMouseButton,
        UOEngineMouseButton.Middle => throw new NotImplementedException(),
        UOEngineMouseButton.Right => throw new NotImplementedException(),
        UOEngineMouseButton.Back => throw new NotImplementedException(),
        UOEngineMouseButton.Forward => throw new NotImplementedException(),
        _ => throw new NotImplementedException(),
    };

    private void UpdateMouseInput(RawPointerEventType eventType)
    {
        Input?.Invoke(new RawPointerEventArgs(_mouse, 0, Owner, eventType, _lastMousePosition, _mouseModifiers));
    }
}
