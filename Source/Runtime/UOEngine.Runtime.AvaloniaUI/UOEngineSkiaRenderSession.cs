// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia.Skia;
using SkiaSharp;

using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.AvaloniaUI;

internal class UOEngineSkiaRenderSession : ISkiaGpuRenderSession
{
    public GRContext GrContext { get; }

    public SKSurface SkSurface { get; }

    public double ScaleFactor => 1.0f;

    public GRSurfaceOrigin SurfaceOrigin => throw new NotImplementedException();

    public UOEngineSkiaRenderSession(GRContext grContext, SKSurface surface)
    {
        GrContext = grContext;
        SkSurface = surface;
    }

    public void Dispose()
    {
        SkSurface.Flush(true);
    }
}
