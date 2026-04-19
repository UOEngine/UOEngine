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
    void Resize(int width, int height);
}

// The runtime game.
public interface IHostedGame
{
    Game Game { get; }
    IHostedGameSurface Surface { get; }
}
