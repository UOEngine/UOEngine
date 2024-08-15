using System.Diagnostics;

using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Vulkan;
using Semaphore = Silk.NET.Vulkan.Semaphore;
using Silk.NET.Core.Contexts;

namespace UOEngine.Runtime.Rendering
{
    public class RenderSwapChain
    {
        public Fence ImagePresentedFence { get; private set; }

        public RenderSwapChain(RenderDevice renderDevice)
        {
            _renderDevice = renderDevice;

            _vk = Vk.GetApi();
        }

        public unsafe void Setup(IVkSurface vkSurface, uint width, uint height, ERenderTextureFormat pixelFormat)
        {
            Console.WriteLine($"CreateSwapChain: {width} {height}");

            if (_vk.TryGetInstanceExtension<KhrSurface>(_renderDevice.Instance, out _khrSurface) != true)
            {
                throw new NotSupportedException("KHR_surface extension not found.");
            }

            _surface = vkSurface.Create<AllocationCallbacks>(_renderDevice.Instance.ToHandle(), null).ToSurface();

            var swapChainSupport = QuerySwapChainSupport(_renderDevice.PhysicalHandle);

            Format vkFormat = RenderCommon.TextureFormatToVulkanFormat(pixelFormat);

            if (swapChainSupport.Formats.Any(x => x.Format == vkFormat) == false)
            {
                throw new NotSupportedException("Requested format not supported.");
            }

            TexFormat = pixelFormat;

            var surfaceFormat = ChooseSwapSurfaceFormat(swapChainSupport.Formats);

            var presentMode = ChoosePresentMode(swapChainSupport.PresentModes);
            var extent = ChooseSwapExtent(swapChainSupport.Capabilities, width, height);

            var imageCount = swapChainSupport.Capabilities.MinImageCount + 1;
            if (swapChainSupport.Capabilities.MaxImageCount > 0 && imageCount > swapChainSupport.Capabilities.MaxImageCount)
            {
                imageCount = swapChainSupport.Capabilities.MaxImageCount;
            }

           _maxFramesInFlight = imageCount;


            SwapchainCreateInfoKHR createInfo = new()
            {
                SType = StructureType.SwapchainCreateInfoKhr,
                Surface = _surface,

                MinImageCount = imageCount,
                ImageFormat = vkFormat,
                ImageColorSpace = surfaceFormat.ColorSpace,
                ImageExtent = extent,
                ImageArrayLayers = 1,
                ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            };

            _renderDevice.SetupPresentQueue(_khrSurface, _surface);

            var queueFamilyIndices = stackalloc[] { _renderDevice.GraphicsQueueFamilyIndex,  _renderDevice.PresentQueueFamilyIndex };

            if (_renderDevice.GraphicsQueueFamilyIndex != _renderDevice.PresentQueueFamilyIndex)
            {
                createInfo = createInfo with
                {
                    ImageSharingMode = SharingMode.Concurrent,
                    QueueFamilyIndexCount = 2,
                    PQueueFamilyIndices = queueFamilyIndices,
                };
            }
            else
            {
                createInfo.ImageSharingMode = SharingMode.Exclusive;
            }

            createInfo = createInfo with
            {
                PreTransform = swapChainSupport.Capabilities.CurrentTransform,
                CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
                PresentMode = presentMode,
                Clipped = true,

                OldSwapchain = default
            };

            if (_vk!.TryGetDeviceExtension(_renderDevice.Instance, _renderDevice.Handle, out _khrSwapChain) == false)
            {
                throw new NotSupportedException("VK_KHR_swapchain extension not found.");
            }

            if (_khrSwapChain!.CreateSwapchain(_renderDevice.Handle, ref createInfo, null, out _swapChain) != Result.Success)
            {
                throw new Exception("failed to create swap chain!");
            }

            _khrSwapChain.GetSwapchainImages(_renderDevice.Handle, _swapChain, ref imageCount, null);

            //_swapChainTextures = new RenderTexture2D[imageCount];

            _swapChainImages = new Image[imageCount];
            _swapChainImageViews = new ImageView[imageCount];
            _imageAvailableSemaphores = new Semaphore[imageCount];

            fixed (Image* ptr = &_swapChainImages[0])
            {
                _khrSwapChain.GetSwapchainImages(_renderDevice.Handle, _swapChain, ref imageCount, ptr);
            }

            _swapChainImageFormat = surfaceFormat.Format;
            _swapChainExtent = extent;

            for (int i = 0; i < imageCount; i++)
            {
                _swapChainImageViews[i] = _renderDevice.CreateImageView(_swapChainImages[i], vkFormat);
            }

            _imageAvailableSemaphores = new Semaphore[imageCount];

            for(int i = 0; i < imageCount; i++)
            {
                _imageAvailableSemaphores[i] = _renderDevice.CreateSemaphore();
            }

        }

        public unsafe void Shutdown()
        {
            //foreach (var framebuffer in _swapChainFramebuffers!)
            //{
            //    _vk.DestroyFramebuffer(_renderDevice.Handle, framebuffer, null);
            //}

            foreach (var imageView in _swapChainImageViews!)
            {
                _vk.DestroyImageView(_renderDevice.Handle, imageView, null);
            }

            foreach(var semaphore in _imageAvailableSemaphores!)
            {
                _vk.DestroySemaphore(_renderDevice.Handle, semaphore, null);
            }
        }

        public uint AcquireImageIndex(out Semaphore imagePresentedSemaphore)
        {
            uint imageIndex = 0;

            var result = _khrSwapChain!.AcquireNextImage(_renderDevice.Device, _swapChain, ulong.MaxValue, _imageAvailableSemaphores![_currentFrame], default, ref imageIndex);

            if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr)// || frameBufferResized)
            {
                //frameBufferResized = false;

                //RecreateSwapChain();

                Debug.Assert(false);
            }
            else if (result != Result.Success && result != Result.SuboptimalKhr)
            {
                throw new Exception("failed to present swap chain image!");
            }

            imagePresentedSemaphore = _imageAvailableSemaphores![_currentFrame];

            //imagePresentedSemaphore = _imageAvailableSemaphores[_currentFrame];

            return imageIndex;

        }

        public unsafe void Present(Semaphore renderingDoneSemaphore)
        {
            // renderingDoneSemaphore = command buffer has drawn all its commands.

            var frameIndex = _currentFrame;
            var swapChains = stackalloc[] { _swapChain };

            PresentInfoKHR presentInfo = new()
            {
                SType = StructureType.PresentInfoKhr,

                WaitSemaphoreCount = 1,
                PWaitSemaphores = &renderingDoneSemaphore,

                SwapchainCount = 1,
                PSwapchains = swapChains,

                PImageIndices = &frameIndex,
            };

            var result = _khrSwapChain!.QueuePresent(_renderDevice!.PresentQueue, ref presentInfo);

            _currentFrame = (_currentFrame + 1) % _maxFramesInFlight;
        }

        //public Framebuffer CurrentSwapChainFrameBuffer => _swapChainFramebuffers![_currentFrame];

        public Extent2D Extent => _swapChainExtent;

        public ERenderTextureFormat TexFormat { get; private set; }

        private unsafe void Cleanup()
        {
            //for (int i = 0; i < _swapChainFramebuffers!.Length; i++)
            //{
            //    _vk!.DestroyFramebuffer(_renderDevice.Device, _swapChainFramebuffers[i], null);
            //}

            for (int i = 0; i < _swapChainImageViews!.Length; i++)
            {
                _vk.DestroyImageView(_renderDevice.Device, _swapChainImageViews[i], null);
            }

            _khrSwapChain!.DestroySwapchain(_renderDevice.Device, _swapChain, null);
        }

        private unsafe SwapChainSupportDetails QuerySwapChainSupport(PhysicalDevice physicalDevice)
        {
            var details = new SwapChainSupportDetails();

            _khrSurface!.GetPhysicalDeviceSurfaceCapabilities(physicalDevice, _surface, out details.Capabilities);

            uint formatCount = 0;
            _khrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, _surface, ref formatCount, null);

            if (formatCount != 0)
            {
                details.Formats = new SurfaceFormatKHR[formatCount];
                fixed (SurfaceFormatKHR* formatsPtr = details.Formats)
                {
                    _khrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, _surface, ref formatCount, formatsPtr);
                }
            }
            else
            {
                details.Formats = Array.Empty<SurfaceFormatKHR>();
            }

            uint presentModeCount = 0;
            _khrSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, _surface, ref presentModeCount, null);

            if (presentModeCount != 0)
            {
                details.PresentModes = new PresentModeKHR[presentModeCount];
                fixed (PresentModeKHR* formatsPtr = details.PresentModes)
                {
                    _khrSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, _surface, ref presentModeCount, formatsPtr);
                }

            }
            else
            {
                details.PresentModes = Array.Empty<PresentModeKHR>();
            }

            return details;
        }

        private SurfaceFormatKHR ChooseSwapSurfaceFormat(IReadOnlyList<SurfaceFormatKHR> availableFormats)
        {
            foreach (var availableFormat in availableFormats)
            {
                if (availableFormat.Format == Format.B8G8R8A8Srgb && availableFormat.ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr)
                {
                    return availableFormat;
                }
            }

            return availableFormats[0];
        }

        private PresentModeKHR ChoosePresentMode(IReadOnlyList<PresentModeKHR> availablePresentModes)
        {
            foreach (var availablePresentMode in availablePresentModes)
            {
                if (availablePresentMode == PresentModeKHR.MailboxKhr)
                {
                    return availablePresentMode;
                }
            }

            return PresentModeKHR.FifoKhr;
        }

        private Extent2D ChooseSwapExtent(SurfaceCapabilitiesKHR capabilities, uint width, uint height)
        {
            if (capabilities.CurrentExtent.Width != uint.MaxValue)
            {
                return capabilities.CurrentExtent;
            }
            else
            {
                Extent2D actualExtent = new()
                {
                    Width = width,
                    Height = height
                };

                actualExtent.Width = Math.Clamp(actualExtent.Width, capabilities.MinImageExtent.Width, capabilities.MaxImageExtent.Width);
                actualExtent.Height = Math.Clamp(actualExtent.Height, capabilities.MinImageExtent.Height, capabilities.MaxImageExtent.Height);

                return actualExtent;
            }
        }

        struct SwapChainSupportDetails
        {
            public SurfaceCapabilitiesKHR Capabilities;
            public SurfaceFormatKHR[] Formats;
            public PresentModeKHR[] PresentModes;
        }

        public ImageView[]          ShaderResourceViews => _swapChainImageViews!;

        private RenderDevice        _renderDevice;

        private KhrSwapchain?       _khrSwapChain;
        private SwapchainKHR        _swapChain;
        private Image[]?            _swapChainImages;
        private Format              _swapChainImageFormat;
        private Extent2D            _swapChainExtent;
        private ImageView[]?        _swapChainImageViews;
        //private Framebuffer[]?      _swapChainFramebuffers;
        //private RenderTexture2D[]    _swapChainTextures;

        private Semaphore[]?        _imageAvailableSemaphores;

        private KhrSurface?         _khrSurface;
        private SurfaceKHR          _surface;

        private Vk                  _vk;

        private uint                 _currentFrame = 0;

        private uint                _maxFramesInFlight = 0;

    }
}
