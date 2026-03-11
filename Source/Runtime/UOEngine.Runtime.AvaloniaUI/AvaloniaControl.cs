// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Platform;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.AvaloniaUI;

internal class AvaloniaControl
{
    private UOEngineTopLevel? _topLevel;

    public void Initialise()
    {
        var locator = AvaloniaLocator.Current;

        if (locator.GetService<IPlatformGraphics>() is not UOEnginePlatformGraphics graphics)
        {
            Console.WriteLine("No UOEngine platform graphics found?");

            return;
        }

        var topLevelImpl = new UOEngineTopLevelImpl(graphics);

        topLevelImpl.SetRenderSize(new PixelSize(1920, 1080));

        _topLevel = new UOEngineTopLevel(topLevelImpl);

        _topLevel.Prepare();
        _topLevel.StartRendering();
    }

    public void Draw(IRenderContext renderContext)
    {
        _topLevel!.OnDraw(renderContext, new Rect(0, 0, _topLevel.ClientSize.Width, _topLevel.ClientSize.Height));
    }
}
