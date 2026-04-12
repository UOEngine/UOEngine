using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Vulkan;
using SkiaSharp;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.AvaloniaUI;

internal class UOEngineExternalObjectsFeature : IExternalObjectsRenderInterfaceContextFeature
{
    private readonly UOEngineSkiaGpu _gpu;

    public IReadOnlyList<string> SupportedImageHandleTypes => throw new NotImplementedException();

    public IReadOnlyList<string> SupportedSemaphoreTypes => throw new NotImplementedException();

    public byte[]? DeviceUuid => null;

    public byte[]? DeviceLuid => null;

    public UOEngineExternalObjectsFeature(UOEngineSkiaGpu gpu)
    {
        _gpu = gpu;
    }

    public CompositionGpuImportedImageSynchronizationCapabilities GetSynchronizationCapabilities(string imageHandleType)
    {
        throw new NotImplementedException();
    }

    public IPlatformRenderInterfaceImportedImage ImportImage(IPlatformHandle handle, PlatformGraphicsExternalImageProperties properties)
    {
        throw new NotImplementedException();
    }

    public IPlatformRenderInterfaceImportedImage ImportImage(ICompositionImportableSharedGpuContextImage image)
    {
        var t = (UOEngineCompositionImportableSharedGpuContextImage)image;

        return new Image(_gpu, t.Texture);
    }

    public IPlatformRenderInterfaceImportedSemaphore ImportSemaphore(IPlatformHandle handle) => new Semaphore();

    class Semaphore : IPlatformRenderInterfaceImportedSemaphore
    {
        private RhiSemaphore? _inner;

        public Semaphore(RhiSemaphore inner)
        {
            _inner = inner;
        }

        public Semaphore()
        {
        }

        public void Dispose()
        {
            _inner?.Dispose();
            _inner = null;
        }

        public RhiSemaphore Inner =>
            _inner ?? throw new ObjectDisposedException(nameof(RhiSemaphore));

    }

    class Image : IPlatformRenderInterfaceImportedImage
    {
        private readonly UOEngineSkiaGpu _gpu;
        private IRenderTexture _texture;

        public Image(UOEngineSkiaGpu gpu, IRenderTexture inner )
        {
            _gpu = gpu;
            _texture = inner;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IBitmapImpl SnapshotWithAutomaticSync()
        {
            _texture.GetFeature<RhiVkImageInterop>(out var vkImageInterop);

            _gpu.GrContext.ResetContext();

            var imageInfo = new GRVkImageInfo
            {
                CurrentQueueFamily = _gpu.GraphicsQueueFamilyIndex,
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

            using var renderTarget = new GRBackendRenderTarget((int)_texture.Width, (int)_texture.Height, imageInfo);
            using var skSurface = SKSurface.Create(
            _gpu.GrContext,
            renderTarget,
            GRSurfaceOrigin.TopLeft,
            SKColorType.Rgba8888,
            new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));
            var image = skSurface.Snapshot();
            _gpu.GrContext.Flush();

            return new UOEngineImmutableBitmap(image);
        }

        public IBitmapImpl SnapshotWithKeyedMutex(uint acquireIndex, uint releaseIndex)
        {
            throw new NotImplementedException();
        }

        public IBitmapImpl SnapshotWithSemaphores(IPlatformRenderInterfaceImportedSemaphore waitForSemaphore, IPlatformRenderInterfaceImportedSemaphore signalSemaphore)
        {
            UOEDebug.NotImplemented();
            throw new NotImplementedException();
        }

        public IBitmapImpl SnapshotWithTimelineSemaphores(IPlatformRenderInterfaceImportedSemaphore waitForSemaphore, ulong waitForValue, IPlatformRenderInterfaceImportedSemaphore signalSemaphore, ulong signalValue)
        {
            throw new NotImplementedException();
        }
    }
}

