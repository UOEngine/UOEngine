// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using CentrED;

using Microsoft.Xna.Framework;
using UOEngine.Runtime.AvaloniaUI;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.RHI;

using UOERectangle = UOEngine.Runtime.Core.Rectangle;

namespace UOEngine.Editor.CentredSharp;

internal class HostedCentrEDGame : IHostedGame
{
    public Game Game { get; private set; }

    public IHostedGameSurface Surface { get; private set; }

    internal HostedCentrEDGame(CentrEDGame game, DrawingSurfaceControl drawingSurfaceControl)
    {
        Game = game;
        Surface = new HostedCentrEDSurface(drawingSurfaceControl);
    }
}

internal class HostedCentrEDSurface : IHostedGameSurface
{
    public RhiRenderTarget? AcquireRenderTarget()
    {
        return _drawingSurfaceControl.RenderTarget;
    }

    public void Resize(int width, int height)
    {
        UOEDebug.NotImplemented();
    }

    public void Present(IRenderContext context)
    {
        _drawingSurfaceControl.Copy(context);
    }

    internal HostedCentrEDSurface(DrawingSurfaceControl drawingSurfaceControl)
    {
        _drawingSurfaceControl = drawingSurfaceControl;
    }

    private DrawingSurfaceControl _drawingSurfaceControl;
}

internal class HostedCentrEDGameHost : IHostedGameHost
{
    public nint NativeWindowHandle { get; private set; }

    public Runtime.Core.Rectangle ClientBounds { get; private set; }

    public event Action<UOERectangle>? BoundsChanged;

    public HostedCentrEDGameHost(nint nativeWindowHandle)
    {
        NativeWindowHandle = nativeWindowHandle;
    }

    public void SetBounds(uint width, uint height)
    {
        var next = new UOERectangle(0, 0, (int)width, (int)height);

        if (next == ClientBounds)
            return;

        ClientBounds = next;
        BoundsChanged?.Invoke(next);
    }

}