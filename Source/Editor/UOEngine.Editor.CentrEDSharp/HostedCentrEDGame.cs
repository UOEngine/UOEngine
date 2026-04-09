// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using CentrED;

using Microsoft.Xna.Framework;

namespace UOEngine.Editor.CentredSharp;

internal class HostedCentrEDGame : CentrEDGame
{
    protected override bool BeginDraw()
    {
        //GraphicsDevice.SetRenderTarget();

        return base.BeginDraw();
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
    }
}