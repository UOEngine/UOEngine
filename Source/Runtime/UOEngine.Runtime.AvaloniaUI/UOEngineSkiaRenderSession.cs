// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia.Skia;
using SkiaSharp;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.AvaloniaUI;

internal class UOEngineSkiaRenderSession : ISkiaGpuRenderSession, IRhiBackbufferRenderParticipant
{
    public GRContext GrContext { get; }

    public SKSurface SkSurface { get; }

    public double ScaleFactor => 1.0f;

    public GRSurfaceOrigin SurfaceOrigin => throw new NotImplementedException();

    //private readonly RHI.IRenderer _renderer;

    private bool _needsFlushing = false;

    public UOEngineSkiaRenderSession(GRContext grContext, SKSurface surface)//, RHI.IRenderer renderer)
    {
        GrContext = grContext;
        SkSurface = surface;
        //_renderer = renderer;
    }

    public void Dispose()
    {
        _needsFlushing = true;
        RenderToCurrentBackbuffer();
    }

    public void RenderToCurrentBackbuffer()
    {
        // This submits to the graphics queue.
        if(_needsFlushing)
        {
            SkSurface.Flush(false);
            _needsFlushing = false;
        }
    }
}
