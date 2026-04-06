// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.AvaloniaUI;

internal class AvaloniaControl
{
    private UOEngineTopLevel? _topLevel;

    private RootControl? _rootControl;

    private IRootContentHost _rootContentHost;

    internal AvaloniaControl(IRootContentHost rootContentHost)
    {
        _rootContentHost = rootContentHost;
    }

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

        _rootControl = new RootControl
        {
            DataContext = _rootContentHost
        };

        _topLevel = new UOEngineTopLevel(topLevelImpl)
        {
            Background = null,
            Content = _rootControl,
            TransparencyLevelHint = [WindowTransparencyLevel.Transparent, WindowTransparencyLevel.None]
        };

        _topLevel.Prepare();
        _topLevel.StartRendering();
    }

    public void Draw(IRenderContext renderContext)
    {
        Dispatcher.UIThread.RunJobs();

        _topLevel!.OnDraw(renderContext, new Rect(0, 0, _topLevel.ClientSize.Width, _topLevel.ClientSize.Height));
    }
}
