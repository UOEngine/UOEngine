// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia.Platform;
using Avalonia.Vulkan;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.AvaloniaUI;

internal class UOEnginePlatformGraphics : IPlatformGraphics
{
    public bool UsesSharedContext => true;

    private UOEngineSkiaGpu? _context;
    private readonly IRenderer _renderer;
    private readonly IRenderResourceFactory _resourceFactory;


    public UOEnginePlatformGraphics(IRenderer renderer, IRenderResourceFactory resourceFactory)
    {
        _renderer = renderer;
        _resourceFactory = resourceFactory;
    }

    public IPlatformGraphicsContext CreateContext() => throw new NotImplementedException();

    public IPlatformGraphicsContext GetSharedContext()
    {
        if(_context == null || _context.IsLost)
        {
            _context?.Dispose();

            _context = new UOEngineSkiaGpu(_renderer, _resourceFactory);
        }

        return _context;
    }
}
