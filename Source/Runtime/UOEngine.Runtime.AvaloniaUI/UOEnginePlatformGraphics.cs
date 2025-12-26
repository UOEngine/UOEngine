// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia.Platform;

namespace UOEngine.Runtime.AvaloniaUI;

internal class UOEnginePlatformGraphics : IPlatformGraphics
{
    public bool UsesSharedContext => true;

    private UOEngineSkiaGpu? _context;

    public IPlatformGraphicsContext CreateContext() => throw new NotImplementedException();

    public UOEngineSkiaGpu GetSharedContext()
    {
        if(_context == null || _context.IsLost)
        {
            _context?.Dispose();

            _context = new UOEngineSkiaGpu();
        }

        return _context;
    }

    IPlatformGraphicsContext IPlatformGraphics.GetSharedContext() => GetSharedContext();
}
