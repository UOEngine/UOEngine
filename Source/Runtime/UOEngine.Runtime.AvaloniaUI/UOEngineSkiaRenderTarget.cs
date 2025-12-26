// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia.Skia;
using SkiaSharp;

namespace UOEngine.Runtime.AvaloniaUI;

internal class UOEngineSkiaRenderTarget : ISkiaGpuRenderTarget
{
    public bool IsCorrupted => false;

    private readonly UOEngineSkiaSurface _surface;
    private readonly GRContext _grContext;

    public UOEngineSkiaRenderTarget(UOEngineSkiaSurface surface, GRContext grContext)
    {
        _surface = surface;
        _grContext = grContext;
    }

    public ISkiaGpuRenderSession BeginRenderingSession()
    {
       return new UOEngineSkiaRenderSession(_grContext, _surface);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }


}
