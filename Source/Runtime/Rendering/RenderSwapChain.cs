using System.Diagnostics;

using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Vulkan;
using Semaphore = Silk.NET.Vulkan.Semaphore;
using Silk.NET.Core.Contexts;

namespace UOEngine.Runtime.Rendering
{
    public class RenderSwapChain
    {
        public RenderSwapChain(RenderDevice renderDevice)
        {
            _renderDevice = renderDevice;

            _vk = Vk.GetApi();
        }

        public unsafe void Setup(IVkSurface vkSurface, uint width, uint height)
        {
            Console.WriteLine($"CreateSwapChain: {width} {height}");

            if (_vk.TryGetInstanceExtension<KhrSurface>(_renderDevice.Instance, out _khrSurface) != true)
            {
                throw new NotSupportedException("KHR_surface extension not found.");
            }

            _surface = vkSurface.Create<AllocationCallbacks>(_renderDevice.Instance.ToHandle(), null).ToSurface();

            var swapChainSupport = QuerySwapChainSupport(_renderDevice.PhysicalHandle);

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
                ImageFormat = surfaceFormat.Format,
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

            _swapChainImages = new Image[imageCount];
            _imageAvailableSemaphores = new Semaphore[imageCount];

            fixed (Image* swapChainImagesPtr = _swapChainImages)
            {
                _khrSwapChain.GetSwapchainImages(_renderDevice.Handle, _swapChain, ref imageCount, swapChainImagesPtr);
            }

            _swapChainImageFormat = surfaceFormat.Format;
            _swapChainExtent = extent;

            _swapChainImageViews = new ImageView[_swapChainImages!.Length];

            for (int i = 0; i < _swapChainImages.Length; i++)
            {
                ImageViewCreateInfo imageViewCreateInfo = new()
                {
                    SType = StructureType.ImageViewCreateInfo,
                    Image = _swapChainImages[i],
                    ViewType = ImageViewType.Type2D,
                    Format = _swapChainImageFormat,
                    Components =
                {
                    R = ComponentSwizzle.Identity,
                    G = ComponentSwizzle.Identity,
                    B = ComponentSwizzle.Identity,
                    A = ComponentSwizzle.Identity,
                },
                    SubresourceRange =
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                }

                };

                if (_vk.CreateImageView(_renderDevice.Device, ref imageViewCreateInfo, null, out _swapChainImageViews[i]) != Result.Success)
                {
                    throw new Exception("failed to create image views!");
                }
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

        public int AcquireImageIndex(out Semaphore semaphore)
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

            _currentFrame = 0;

            semaphore = _imageAvailableSemaphores[_currentFrame];

            return (int)imageIndex;

        }

        public unsafe void Present(Queue presentQueue, Semaphore renderingDoneSemaphore)
        {
            var frameIndex = (uint)_currentFrame;
            var swapChains = stackalloc[] { _swapChain };

            PresentInfoKHR presentInfo = new()
            {
                SType = StructureType.PresentInfoKhr,

                WaitSemaphoreCount = 1,
                PWaitSemaphores = &renderingDoneSemaphore,

                SwapchainCount = 1,
                PSwapchains = swapChains,

                PImageIndices = &frameIndex
            };

            var result = _khrSwapChain!.QueuePresent(presentQueue, ref presentInfo);
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

        private RenderDevice        _renderDevice;

        private KhrSwapchain?       _khrSwapChain;
        private SwapchainKHR        _swapChain;
        private Image[]?            _swapChainImages;
        private Format              _swapChainImageFormat;
        private Extent2D            _swapChainExtent;
        private ImageView[]?        _swapChainImageViews;
        //private Framebuffer[]?      _swapChainFramebuffers;

        private Semaphore[]?        _imageAvailableSemaphores;

        private KhrSurface?         _khrSurface;
        private SurfaceKHR          _surface;

        private Vk                  _vk;

        private int                 _currentFrame;

        private uint                _maxFramesInFlight = 0;

    }
}
