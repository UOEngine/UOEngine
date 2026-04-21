// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Microsoft.Xna.Framework;

using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.FnaAdapter;

// The surface we want to draw to.
public interface IHostedGameSurface
{
    RhiRenderTarget? AcquireRenderTarget();
    void Present(IRenderContext context);
}

// The runtime game and the surface it renders into.
public interface IHostedGame
{
    Game Game { get; }
    IHostedGameSurface Surface { get; }
}
