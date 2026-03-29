// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using Avalonia.Platform;
using Avalonia.Skia;
using SkiaSharp;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.AvaloniaUI;

internal class UOEngineSkiaRenderTarget : ISkiaGpuRenderTarget
{
    public bool IsCorrupted => false;

    private readonly IRenderTexture _texture;
    private readonly GRContext _grContext;
    private readonly uint _queueFamilyIndex;

    public UOEngineSkiaRenderTarget(IRenderTexture texture, GRContext grContext, uint queueFamilyIndex)
    {
        _texture = texture;
        _grContext = grContext;
        _queueFamilyIndex = queueFamilyIndex;
    }

    public ISkiaGpuRenderSession BeginRenderingSession(IRenderTarget.RenderTargetSceneInfo sceneInfo)
    {

        _texture.GetFeature<RhiVkImageInterop>(out var vkImageInterop);

        var imageInfo = new GRVkImageInfo
        {
            CurrentQueueFamily = _queueFamilyIndex,
            LevelCount = 1,
            SampleCount = 1,
            Protected = false,
            Format = vkImageInterop!.Format,
            Image = vkImageInterop.Handle,
            ImageLayout = vkImageInterop.ImageLayout,
            ImageTiling = vkImageInterop.ImageTiling,
            ImageUsageFlags = vkImageInterop.ImageUsageFlags,
            SharingMode = vkImageInterop.SharingMode
        };

        var renderTarget = new GRBackendRenderTarget((int)_texture.Width, (int)_texture.Height, imageInfo);

        var skSurface = SKSurface.Create(
            _grContext,
            renderTarget,
            GRSurfaceOrigin.BottomLeft,
            SKColorType.Rgba8888,
            new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal)
        );

        if (skSurface is null)
            throw new InvalidOperationException("Failed to create Vulkan-backed SKSurface");

        return new UOEngineSkiaRenderSession(_grContext, skSurface);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
