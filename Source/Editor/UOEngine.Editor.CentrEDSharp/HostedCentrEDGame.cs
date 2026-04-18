// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using CentrED;

using Microsoft.Xna.Framework;
using UOEngine.Runtime.AvaloniaUI;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.RHI;

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