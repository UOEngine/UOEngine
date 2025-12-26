// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia.Skia;
using SkiaSharp;

namespace UOEngine.Runtime.AvaloniaUI;

internal class UOEngineSkiaRenderSession : ISkiaGpuRenderSession
{
    public GRContext GrContext { get; }

    public SKSurface SkSurface { get; }

    public double ScaleFactor => 1.0f;

    public GRSurfaceOrigin SurfaceOrigin => throw new NotImplementedException();

    public UOEngineSkiaRenderSession(GRContext grContext, UOEngineSkiaSurface surface)
    {
        GrContext = grContext;
        SkSurface = surface.Surface;
    }

    public void Dispose()
    {
        SkSurface.Flush(true);
    }
}
