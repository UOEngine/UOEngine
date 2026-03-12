// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia.Controls.Platform.Surfaces;
using Avalonia.Skia;
using SkiaSharp;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.AvaloniaUI;

internal class UOEngineSkiaSurface : ISkiaSurface
{
    public SKSurface Surface { get; }

    public bool CanBlit => false;

    public readonly IRenderTexture RhiTexture;

    public UOEngineSkiaSurface(SKSurface surface, IRenderTexture rhiTexture)
    {
        RhiTexture = rhiTexture;
        Surface = surface;
    }

    public void Blit(SKCanvas canvas) => throw new NotImplementedException();

    public void Dispose()
    {
        Surface.Dispose();
    }
}
