// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;

namespace UOEngine.Runtime.AvaloniaUI;

internal class UOEnginePlatform
{
    public static Compositor Compositor
        => _compositor ?? throw new InvalidOperationException("Compositor has not been initialized");

    private static Compositor? _compositor;

    public static void Initialise()
    {
        AvaloniaSynchronizationContext.AutoInstall = false;

        var renderTimer = new ManualRenderTimer();
        var platformGraphics = new UOEnginePlatformGraphics();

        AvaloniaLocator.CurrentMutable
            .Bind<IRenderTimer>().ToConstant(renderTimer)
            .Bind<IPlatformGraphics>().ToConstant(platformGraphics);


        _compositor = new Compositor(platformGraphics);
    }
}
