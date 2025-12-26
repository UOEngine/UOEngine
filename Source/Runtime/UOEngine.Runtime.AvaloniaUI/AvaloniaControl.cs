// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Platform;

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

        _topLevel = new UOEngineTopLevel(topLevelImpl);

        _topLevel.Prepare();
        _topLevel.StartRendering();
    }

    public void Draw()
    {
        _topLevel!.OnDraw(new Avalonia.Rect(0, 0, 1, 1));
    }
}
