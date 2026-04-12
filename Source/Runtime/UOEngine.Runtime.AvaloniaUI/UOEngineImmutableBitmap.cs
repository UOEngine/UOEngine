using Avalonia;
using Avalonia.Platform;
using Avalonia.Skia;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace UOEngine.Runtime.AvaloniaUI;

internal class UOEngineImmutableBitmap : IDrawableBitmapImpl
{
    public Vector Dpi { get; }
    public PixelSize PixelSize { get; }

    public int Version { get; } = 1;

    private readonly SKImage _image;

    public UOEngineImmutableBitmap(SKImage image)
    {
        _image = image;
        PixelSize = new PixelSize(image.Width, image.Height);
        Dpi = new Vector(96, 96);
    }

    public void Dispose()
    {
        _image.Dispose();
    }

    public void Save(string fileName, int? quality = null)
    {
        throw new NotImplementedException();
    }

    public void Save(Stream stream, int? quality = null)
    {
        throw new NotImplementedException();
    }

    public void Draw(DrawingContextImpl context, SKRect sourceRect, SKRect destRect, SKSamplingOptions samplingOptions, SKPaint paint)
    {
        context.Canvas.DrawImage(_image, sourceRect, destRect, samplingOptions, paint);
    }
}
