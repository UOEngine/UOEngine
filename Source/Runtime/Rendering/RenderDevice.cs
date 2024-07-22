using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Silk.NET.Core;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace UOEngine.Runtime.Rendering
{
    public class RenderDevice
    {
        public RenderDevice(RenderDeviceContext renderDeviceContext)
        {
            this.renderDeviceContext = renderDeviceContext;
            renderDeviceContext.RenderDevice = this;
        }

        public unsafe void Initialise(IVkSurface? surface, uint width, uint height, bool bEnableValidationLayers)
        {
            this.bEnableValidationLayers = bEnableValidationLayers;

            if (surface == null)
            {
                throw new ArgumentNullException("Surface is null.");
            }

            surfaceWidth = width;
            surfaceHeight = height;

            vk = Vk.GetApi();

            ApplicationInfo appInfo = new()
            {
                SType = StructureType.ApplicationInfo,
                PApplicationName = (byte*)Marshal.StringToHGlobalAnsi("UOEngine.App"),
                ApplicationVersion = new Version32(1, 0, 0),
                PEngineName = (byte*)Marshal.StringToHGlobalAnsi("UOEngine"),
                EngineVersion = new Version32(1, 0, 0),
                ApiVersion = Vk.Version11
            };

            InstanceCreateInfo createInfo = new()
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo
            };

            var extensions = GetRequiredExtensions(surface, bEnableValidationLayers);

            createInfo.EnabledExtensionCount = (uint)extensions.Length;
            createInfo.PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(extensions);
            createInfo.EnabledLayerCount = 0;

            if (bEnableValidationLayers)
            {
                createInfo.EnabledLayerCount = (uint)validationLayers.Length;
                createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(validationLayers);

                DebugUtilsMessengerCreateInfoEXT debugCreateInfo = new();
                PopulateDebugMessengerCreateInfo(ref debugCreateInfo);
                createInfo.PNext = &debugCreateInfo;
            }
            else
            {
                createInfo.EnabledLayerCount = 0;
                createInfo.PNext = null;
            }

            if (vk.CreateInstance(ref createInfo, null, out instance) != Result.Success)
            {
                throw new Exception("failed to create instance!");
            }

            Marshal.FreeHGlobal((IntPtr)appInfo.PApplicationName);
            Marshal.FreeHGlobal((IntPtr)appInfo.PEngineName);

            if(bEnableValidationLayers)
            {
                SetupDebugMessenger();
            }

            CreateSurface(surface);

            PickPhysicalDevice();
            CreateLogicalDevice(bEnableValidationLayers);
            CreateSwapChain(width, height);
            CreateRenderPass();
            CreateImageViews();
            CreateGraphicsPipeline();
            CreateFramebuffers();
            CreateCommandPool();
            CreateCommandBuffers();
            CreateSyncObjects();

        }

        public unsafe void Shutdown()
        {
            vk!.DeviceWaitIdle(_dev);

            CleanupSwapchain();

            for (var i = 0; i < MaxFramesInFlight; i++)
            {
                vk.DestroySemaphore(_dev, renderFinishedSemaphores![i], null);
                vk!.DestroySemaphore(_dev, imageAvailableSemaphores![i], null);
                vk!.DestroyFence(_dev, inFlightFences![i], null);
            }

            vk!.DestroyCommandPool(_dev, commandPool, null);

            foreach (var framebuffer in swapChainFramebuffers!)
            {
                vk!.DestroyFramebuffer(_dev, framebuffer, null);
            }

            vk!.DestroyPipeline(_dev, graphicsPipeline, null);
            vk!.DestroyPipelineLayout(_dev, pipelineLayout, null);
            vk!.DestroyRenderPass(_dev, renderPass, null);

            foreach (var imageView in swapChainImageViews!)
            {
                vk!.DestroyImageView(_dev, imageView, null);
            }

            vk!.DestroyDevice(_dev, null);

            if (bEnableValidationLayers)
            {
                debugUtils!.DestroyDebugUtilsMessenger(instance, debugMessenger, null);
            }

            khrSurface!.DestroySurface(instance, surface, null);
            vk!.DestroyInstance(instance, null);

            vk!.Dispose();
        }

        public void SetSurfaceSize(uint width, uint height)
        {
            Console.WriteLine($"SetSurfaceSize: {width} {height}");
            surfaceWidth = width; 
            surfaceHeight = height;
        }
        public void OnFrameBegin()
        {
            Debug.WriteLine($"OnFrameBegin {currentFrame}");

            currentCommandBuffer = commandBuffers![currentFrame];

            vk!.ResetCommandBuffer(currentCommandBuffer, 0);

            CommandBufferBeginInfo beginInfo = new()
            {
                SType = StructureType.CommandBufferBeginInfo,
            };

            if (vk!.BeginCommandBuffer(currentCommandBuffer, ref beginInfo) != Result.Success)
            {
                throw new Exception("failed to begin recording command buffer!");
            }
        }

        public unsafe void BeginRenderPass()
        {
            RenderPassBeginInfo renderPassInfo = new()
            {
                SType = StructureType.RenderPassBeginInfo,
                RenderPass = renderPass,
                Framebuffer = swapChainFramebuffers![currentFrame],
                RenderArea =
                {
                    Offset = { X = 0, Y = 0 },
                    Extent = swapChainExtent,
                }
            };

            ClearValue clearColor = new()
            {
                Color = new() { Float32_0 = 0, Float32_1 = 0, Float32_2 = 0, Float32_3 = 1 },
            };

            renderPassInfo.ClearValueCount = 1;
            renderPassInfo.PClearValues = &clearColor;

            vk!.CmdBeginRenderPass(currentCommandBuffer, &renderPassInfo, SubpassContents.Inline);

            vk!.CmdBindPipeline(currentCommandBuffer, PipelineBindPoint.Graphics, graphicsPipeline);

            Viewport viewport = new()
            {
                X = 0,
                Y = 0,
                Width = swapChainExtent.Width,
                Height = swapChainExtent.Height,
                MinDepth = 0.0f,
                MaxDepth = 1.0f
            };

            vk.CmdSetViewport(currentCommandBuffer, 0, 1, ref viewport);

            Rect2D scissor = new()
            {
                Offset = new() { X = 0, Y = 0 },
                Extent = swapChainExtent
            };

            vk.CmdSetScissor(currentCommandBuffer, 0, 1, ref scissor);
        }

        public void EndRenderPass()
        {
            vk!.CmdEndRenderPass(currentCommandBuffer);
        }

        public void Draw()
        {
            vk!.CmdDraw(currentCommandBuffer, 3, 1, 0, 0);
        }

        public unsafe void Submit()
        {
            if (vk!.EndCommandBuffer(currentCommandBuffer) != Result.Success)
            {
                throw new Exception("failed to record command buffer!");
            }

            var result = vk!.WaitForFences(_dev, 1, ref inFlightFences![currentFrame], true, ulong.MaxValue);

            uint imageIndex = 0;
            result = khrSwapChain!.AcquireNextImage(_dev, swapChain, ulong.MaxValue, imageAvailableSemaphores![currentFrame], default, ref imageIndex);

            if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr)// || frameBufferResized)
            {
                //frameBufferResized = false;

                RecreateSwapChain();

                return;
            }
            else if (result != Result.Success && result != Result.SuboptimalKhr)
            {
                throw new Exception("failed to present swap chain image!");
            }

            result = vk!.ResetFences(_dev, 1, ref inFlightFences[currentFrame]);

            SubmitInfo submitInfo = new()
            {
                SType = StructureType.SubmitInfo,
            };

            var waitSemaphores = stackalloc[] { imageAvailableSemaphores[currentFrame] };
            var waitStages = stackalloc[] { PipelineStageFlags.ColorAttachmentOutputBit };

            var buffer = currentCommandBuffer;

            submitInfo = submitInfo with
            {
                WaitSemaphoreCount = 1,
                PWaitSemaphores = waitSemaphores,
                PWaitDstStageMask = waitStages,

                CommandBufferCount = 1,
                PCommandBuffers = &buffer
            };

            var signalSemaphores = stackalloc[] { renderFinishedSemaphores![currentFrame] };
            submitInfo = submitInfo with
            {
                SignalSemaphoreCount = 1,
                PSignalSemaphores = signalSemaphores,
            };

            if (vk!.QueueSubmit(graphicsQueue, 1, ref submitInfo, inFlightFences[currentFrame]) != Result.Success)
            {
                throw new Exception("failed to submit draw command buffer!");
            }

            var swapChains = stackalloc[] { swapChain };

            PresentInfoKHR presentInfo = new()
            {
                SType = StructureType.PresentInfoKhr,

                WaitSemaphoreCount = 1,
                PWaitSemaphores = signalSemaphores,

                SwapchainCount = 1,
                PSwapchains = swapChains,

                PImageIndices = &imageIndex
            };

            result = khrSwapChain.QueuePresent(presentQueue, ref presentInfo);

            currentFrame = (currentFrame + 1) % MaxFramesInFlight;

            vk.DeviceWaitIdle(_dev);
        }

        public unsafe CommandBuffer BeginUpload()
        {
            Debug.Assert(_uploadCommandBuffer == null);

            CommandBufferAllocateInfo allocInfo = new()
            {
                SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = commandPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = 1,
            };

            if (vk!.AllocateCommandBuffers(_dev, ref allocInfo, out var commandBuffer) != Result.Success)
            {
                throw new Exception("failed to allocate command buffers!");
            }

            CommandBufferBeginInfo beginInfo = new()
            {
                SType = StructureType.CommandBufferBeginInfo,
                Flags = CommandBufferUsageFlags.OneTimeSubmitBit,
            };

            vk!.BeginCommandBuffer(commandBuffer, ref beginInfo);

            _uploadCommandBuffer = commandBuffer;

            return _uploadCommandBuffer.Value;
        }

        public unsafe void EndUpload()
        {
            Debug.Assert(_uploadCommandBuffer != null);

            var commandBuffer = _uploadCommandBuffer.Value;

            vk!.EndCommandBuffer(commandBuffer);

            SubmitInfo submitInfo = new()
            {
                SType = StructureType.SubmitInfo,
                CommandBufferCount = 1,
                PCommandBuffers = &commandBuffer,
            };

            vk!.QueueSubmit(graphicsQueue, 1, ref submitInfo, default);
            vk!.QueueWaitIdle(graphicsQueue);

            vk!.FreeCommandBuffers(_dev, commandPool, 1, ref commandBuffer);

            _uploadCommandBuffer = null;

        }

        public RenderTexture2D CreateTexture2D(RenderTexture2DDescription description, byte[] texels)
        {
            return new RenderTexture2D(description, texels, this);
        }

        public RenderBuffer CreateRenderBuffer<T>(T[] data, ERenderBufferType usageFlags)
        {
            var renderBuffer = new RenderBuffer(usageFlags, this);

            renderBuffer.CopyToDevice(new ReadOnlySpan<T>(data));

            return renderBuffer;
        }

        private void PickPhysicalDevice()
        {
            var devices = vk!.GetPhysicalDevices(instance);

            foreach (var _dev in devices)
            {
                if (IsDeviceSuitable(_dev))
                {
                    physicalDevice = _dev;
                    break;
                }
            }

            if (physicalDevice.Handle == 0)
            {
                throw new Exception("failed to find a suitable GPU!");
            }
        }

        private unsafe void CreateLogicalDevice(bool bEnableValidationLayers)
        {
            var indices = FindQueueFamilies(physicalDevice);

            var uniqueQueueFamilies = new[] { indices.GraphicsFamily!.Value, indices.PresentFamily!.Value };
            uniqueQueueFamilies = uniqueQueueFamilies.Distinct().ToArray();

            using var mem = GlobalMemory.Allocate(uniqueQueueFamilies.Length * sizeof(DeviceQueueCreateInfo));
            var queueCreateInfos = (DeviceQueueCreateInfo*)Unsafe.AsPointer(ref mem.GetPinnableReference());

            float queuePriority = 1.0f;
            for (int i = 0; i < uniqueQueueFamilies.Length; i++)
            {
                queueCreateInfos[i] = new()
                {
                    SType = StructureType.DeviceQueueCreateInfo,
                    QueueFamilyIndex = uniqueQueueFamilies[i],
                    QueueCount = 1,
                    PQueuePriorities = &queuePriority
                };
            }

            PhysicalDeviceFeatures deviceFeatures = new();

            DeviceCreateInfo createInfo = new()
            {
                SType = StructureType.DeviceCreateInfo,
                QueueCreateInfoCount = (uint)uniqueQueueFamilies.Length,
                PQueueCreateInfos = queueCreateInfos,

                PEnabledFeatures = &deviceFeatures,

                EnabledExtensionCount = (uint)deviceExtensions.Length,
                PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(deviceExtensions)
            };

            if (bEnableValidationLayers)
            {
                createInfo.EnabledLayerCount = (uint)validationLayers.Length;
                createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(validationLayers);
            }
            else
            {
                createInfo.EnabledLayerCount = 0;
            }

            if (vk!.CreateDevice(physicalDevice, ref createInfo, null, out _dev) != Result.Success)
            {
                throw new Exception("failed to create logical device!");
            }

            vk!.GetDeviceQueue(_dev, indices.GraphicsFamily!.Value, 0, out graphicsQueue);
            vk!.GetDeviceQueue(_dev, indices.PresentFamily!.Value, 0, out presentQueue);

            if (bEnableValidationLayers)
            {
                SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
            }

            SilkMarshal.Free((nint)createInfo.PpEnabledExtensionNames);
        }
        private unsafe void CreateSurface(IVkSurface vkSurface)
        {
            if (!vk!.TryGetInstanceExtension<KhrSurface>(instance, out khrSurface))
            {
                throw new NotSupportedException("KHR_surface extension not found.");
            }

            surface = vkSurface.Create<AllocationCallbacks>(instance.ToHandle(), null).ToSurface();
        }

        private unsafe void CreateSwapChain(uint width, uint height)
        {
            Console.WriteLine($"CreateSwapChain: {width} {height}");

            var swapChainSupport = QuerySwapChainSupport(physicalDevice);

            var surfaceFormat = ChooseSwapSurfaceFormat(swapChainSupport.Formats);
            var presentMode = ChoosePresentMode(swapChainSupport.PresentModes);
            var extent = ChooseSwapExtent(swapChainSupport.Capabilities, width, height);

            var imageCount = swapChainSupport.Capabilities.MinImageCount + 1;
            if (swapChainSupport.Capabilities.MaxImageCount > 0 && imageCount > swapChainSupport.Capabilities.MaxImageCount)
            {
                imageCount = swapChainSupport.Capabilities.MaxImageCount;
            }

            MaxFramesInFlight = imageCount;

            SwapchainCreateInfoKHR createInfo = new()
            {
                SType = StructureType.SwapchainCreateInfoKhr,
                Surface = surface,

                MinImageCount = imageCount,
                ImageFormat = surfaceFormat.Format,
                ImageColorSpace = surfaceFormat.ColorSpace,
                ImageExtent = extent,
                ImageArrayLayers = 1,
                ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            };

            var indices = FindQueueFamilies(physicalDevice);
            var queueFamilyIndices = stackalloc[] { indices.GraphicsFamily!.Value, indices.PresentFamily!.Value };

            if (indices.GraphicsFamily != indices.PresentFamily)
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

            if (!vk!.TryGetDeviceExtension(instance, _dev, out khrSwapChain))
            {
                throw new NotSupportedException("VK_KHR_swapchain extension not found.");
            }

            if (khrSwapChain!.CreateSwapchain(_dev, ref createInfo, null, out swapChain) != Result.Success)
            {
                throw new Exception("failed to create swap chain!");
            }

            khrSwapChain.GetSwapchainImages(_dev, swapChain, ref imageCount, null);
            swapChainImages = new Image[imageCount];
            fixed (Image* swapChainImagesPtr = swapChainImages)
            {
                khrSwapChain.GetSwapchainImages(_dev, swapChain, ref imageCount, swapChainImagesPtr);
            }

            swapChainImageFormat = surfaceFormat.Format;
            swapChainExtent = extent;
        }

        private unsafe void CleanupSwapchain()
        {
            for (int i = 0; i < swapChainFramebuffers!.Length; i++)
            {
                vk!.DestroyFramebuffer(_dev, swapChainFramebuffers[i], null);
            }

            for (int i = 0; i < swapChainImageViews!.Length; i++)
            {
                vk!.DestroyImageView(_dev, swapChainImageViews[i], null);
            }

            khrSwapChain!.DestroySwapchain(_dev, swapChain, null);
        }

        private unsafe void CreateImageViews()
        {
            swapChainImageViews = new ImageView[swapChainImages!.Length];

            for (int i = 0; i < swapChainImages.Length; i++)
            {
                ImageViewCreateInfo imageViewCreateInfo = new()
                {
                    SType = StructureType.ImageViewCreateInfo,
                    Image = swapChainImages[i],
                    ViewType = ImageViewType.Type2D,
                    Format = swapChainImageFormat,
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

                if (vk!.CreateImageView(_dev, ref imageViewCreateInfo, null, out swapChainImageViews[i]) != Result.Success)
                {
                    throw new Exception("failed to create image views!");
                }
            }
        }

        private unsafe void CreateGraphicsPipeline()
        {
            var vertShaderCode = File.ReadAllBytes("shaders/vert.spv");
            var fragShaderCode = File.ReadAllBytes("shaders/frag.spv");

            var vertexShaderModule = CreateShaderModule(vertShaderCode);
            var fragmentShaderModule = CreateShaderModule(fragShaderCode);

            PipelineShaderStageCreateInfo vertShaderStageInfo = new()
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.VertexBit,
                Module = vertexShaderModule,
                PName = (byte*)SilkMarshal.StringToPtr("main")
            };

            PipelineShaderStageCreateInfo fragShaderStageInfo = new()
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.FragmentBit,
                Module = fragmentShaderModule,
                PName = (byte*)SilkMarshal.StringToPtr("main")
            };

            var shaderStages = stackalloc[]
            {
                vertShaderStageInfo,
                fragShaderStageInfo
             };

            PipelineVertexInputStateCreateInfo vertexInputInfo = new()
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                VertexBindingDescriptionCount = 0,
                VertexAttributeDescriptionCount = 0,
            };

            PipelineInputAssemblyStateCreateInfo inputAssembly = new()
            {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = PrimitiveTopology.TriangleList,
                PrimitiveRestartEnable = false,
            };

            Viewport viewport = new()
            {
                X = 0,
                Y = 0,
                Width = swapChainExtent.Width,
                Height = swapChainExtent.Height,
                MinDepth = 0,
                MaxDepth = 1,
            };

            Rect2D scissor = new()
            {
                Offset = { X = 0, Y = 0 },
                Extent = swapChainExtent,
            };

            PipelineViewportStateCreateInfo viewportState = new()
            {
                SType = StructureType.PipelineViewportStateCreateInfo,
                ViewportCount = 1,
                PViewports = &viewport,
                ScissorCount = 1,
                PScissors = &scissor,
            };

            PipelineRasterizationStateCreateInfo rasterizer = new()
            {
                SType = StructureType.PipelineRasterizationStateCreateInfo,
                DepthClampEnable = false,
                RasterizerDiscardEnable = false,
                PolygonMode = PolygonMode.Fill,
                LineWidth = 1,
                CullMode = CullModeFlags.BackBit,
                FrontFace = FrontFace.Clockwise,
                DepthBiasEnable = false,
            };

            PipelineMultisampleStateCreateInfo multisampling = new()
            {
                SType = StructureType.PipelineMultisampleStateCreateInfo,
                SampleShadingEnable = false,
                RasterizationSamples = SampleCountFlags.Count1Bit,
            };

            PipelineColorBlendAttachmentState colorBlendAttachment = new()
            {
                ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | ColorComponentFlags.BBit | ColorComponentFlags.ABit,
                BlendEnable = false,
            };

            PipelineColorBlendStateCreateInfo colorBlending = new()
            {
                SType = StructureType.PipelineColorBlendStateCreateInfo,
                LogicOpEnable = false,
                LogicOp = LogicOp.Copy,
                AttachmentCount = 1,
                PAttachments = &colorBlendAttachment,
            };

            colorBlending.BlendConstants[0] = 0;
            colorBlending.BlendConstants[1] = 0;
            colorBlending.BlendConstants[2] = 0;
            colorBlending.BlendConstants[3] = 0;

            PipelineLayoutCreateInfo pipelineLayoutInfo = new()
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = 0,
                PushConstantRangeCount = 0,
            };

            if (vk!.CreatePipelineLayout(_dev, ref pipelineLayoutInfo, null, out pipelineLayout) != Result.Success)
            {
                throw new Exception("failed to create pipeline layout!");
            }

            var dynamicStates = stackalloc[] { DynamicState.Viewport, DynamicState.Scissor };

            PipelineDynamicStateCreateInfo dynamicStateCreateInfo = new()
            {
                SType = StructureType.PipelineDynamicStateCreateInfo,
                DynamicStateCount = 2,
                PDynamicStates = dynamicStates
            };

            GraphicsPipelineCreateInfo pipelineInfo = new()
            {
                SType = StructureType.GraphicsPipelineCreateInfo,
                StageCount = 2,
                PStages = shaderStages,
                PVertexInputState = &vertexInputInfo,
                PInputAssemblyState = &inputAssembly,
                PViewportState = &viewportState,
                PRasterizationState = &rasterizer,
                PMultisampleState = &multisampling,
                PColorBlendState = &colorBlending,
                Layout = pipelineLayout,
                RenderPass = renderPass,
                PDynamicState = &dynamicStateCreateInfo,
                Subpass = 0,
                BasePipelineHandle = default
            };

            if (vk!.CreateGraphicsPipelines(_dev, default, 1, ref pipelineInfo, null, out graphicsPipeline) != Result.Success)
            {
                throw new Exception("failed to create graphics pipeline!");
            }

            vk!.DestroyShaderModule(_dev, vertexShaderModule, null);
            vk!.DestroyShaderModule(_dev, fragmentShaderModule, null);

            SilkMarshal.Free((nint)vertShaderStageInfo.PName);
            SilkMarshal.Free((nint)fragShaderStageInfo.PName);
        }

        private unsafe void CreateRenderPass()
        {
            AttachmentDescription colorAttachment = new()
            {
                Format = swapChainImageFormat,
                Samples = SampleCountFlags.Count1Bit,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.PresentSrcKhr,
            };

            AttachmentReference colorAttachmentRef = new()
            {
                Attachment = 0,
                Layout = ImageLayout.ColorAttachmentOptimal,
            };

            SubpassDescription subpass = new()
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                ColorAttachmentCount = 1,
                PColorAttachments = &colorAttachmentRef,
            };

            SubpassDependency dependency = new()
            {
                SrcSubpass = Vk.SubpassExternal,
                DstSubpass = 0,
                SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
                SrcAccessMask = 0,
                DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
                DstAccessMask = AccessFlags.ColorAttachmentWriteBit
            };

            RenderPassCreateInfo renderPassInfo = new()
            {
                SType = StructureType.RenderPassCreateInfo,
                AttachmentCount = 1,
                PAttachments = &colorAttachment,
                SubpassCount = 1,
                PSubpasses = &subpass,
                DependencyCount = 1,
                PDependencies = &dependency
            };

            if (vk!.CreateRenderPass(_dev, ref renderPassInfo, null, out renderPass) != Result.Success)
            {
                throw new Exception("failed to create render pass!");
            }
        }

        private unsafe void CreateFramebuffers()
        {
            swapChainFramebuffers = new Framebuffer[swapChainImageViews!.Length];

            for (int i = 0; i < swapChainImageViews.Length; i++)
            {
                var attachment = swapChainImageViews[i];

                FramebufferCreateInfo framebufferInfo = new()
                {
                    SType = StructureType.FramebufferCreateInfo,
                    RenderPass = renderPass,
                    AttachmentCount = 1,
                    PAttachments = &attachment,
                    Width = swapChainExtent.Width,
                    Height = swapChainExtent.Height,
                    Layers = 1,
                };

                if (vk!.CreateFramebuffer(_dev, ref framebufferInfo, null, out swapChainFramebuffers[i]) != Result.Success)
                {
                    throw new Exception("failed to create framebuffer!");
                }
            }
        }

        private unsafe ShaderModule CreateShaderModule(byte[] code)
        {
            ShaderModuleCreateInfo createInfo = new()
            {
                SType = StructureType.ShaderModuleCreateInfo,
                CodeSize = (nuint)code.Length,
            };

            ShaderModule shaderModule;

            fixed (byte* codePtr = code)
            {
                createInfo.PCode = (uint*)codePtr;

                if (vk!.CreateShaderModule(_dev, ref createInfo, null, out shaderModule) != Result.Success)
                {
                    throw new Exception();
                }
            }

            return shaderModule;
        }
        private void RecreateSwapChain()
        {
            Console.WriteLine("RecreateSwapChain");

            vk!.DeviceWaitIdle(_dev);

            CleanupSwapchain();

            SwapchainDirty?.Invoke(this, EventArgs.Empty);

            CreateSwapChain(surfaceWidth, surfaceHeight);
            CreateImageViews();
            CreateFramebuffers();
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

        private unsafe SwapChainSupportDetails QuerySwapChainSupport(PhysicalDevice physicalDevice)
        {
            var details = new SwapChainSupportDetails();

            khrSurface!.GetPhysicalDeviceSurfaceCapabilities(physicalDevice, surface, out details.Capabilities);

            uint formatCount = 0;
            khrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, surface, ref formatCount, null);

            if (formatCount != 0)
            {
                details.Formats = new SurfaceFormatKHR[formatCount];
                fixed (SurfaceFormatKHR* formatsPtr = details.Formats)
                {
                    khrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, surface, ref formatCount, formatsPtr);
                }
            }
            else
            {
                details.Formats = Array.Empty<SurfaceFormatKHR>();
            }

            uint presentModeCount = 0;
            khrSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, surface, ref presentModeCount, null);

            if (presentModeCount != 0)
            {
                details.PresentModes = new PresentModeKHR[presentModeCount];
                fixed (PresentModeKHR* formatsPtr = details.PresentModes)
                {
                    khrSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, surface, ref presentModeCount, formatsPtr);
                }

            }
            else
            {
                details.PresentModes = Array.Empty<PresentModeKHR>();
            }

            return details;
        }
        private bool IsDeviceSuitable(PhysicalDevice device)
        {
            var indices = FindQueueFamilies(device);

            return indices.IsComplete();
        }

        private unsafe QueueFamilyIndices FindQueueFamilies(PhysicalDevice device)
        {
            var indices = new QueueFamilyIndices();

            uint queueFamilityCount = 0;
            vk!.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilityCount, null);

            var queueFamilies = new QueueFamilyProperties[queueFamilityCount];
            fixed (QueueFamilyProperties* queueFamiliesPtr = queueFamilies)
            {
                vk!.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilityCount, queueFamiliesPtr);
            }

            uint i = 0;

            foreach (var queueFamily in queueFamilies)
            {
                if (queueFamily.QueueFlags.HasFlag(QueueFlags.GraphicsBit))
                {
                    indices.GraphicsFamily = i;
                }

                khrSurface!.GetPhysicalDeviceSurfaceSupport(device, i, surface, out var presentSupport);

                if (presentSupport)
                {
                    indices.PresentFamily = i;
                }

                if (indices.IsComplete())
                {
                    break;
                }

                i++;
            }

            return indices;
        }

        public uint FindMemoryType(uint typeFilter, MemoryPropertyFlags properties)
        {
            vk!.GetPhysicalDeviceMemoryProperties(physicalDevice, out var memoryProperties);

            for (uint i = 0; i < memoryProperties.MemoryTypeCount; i++)
            {
                if (((typeFilter & (1 << (int)i)) != 0) && (memoryProperties.MemoryTypes[(int)i].PropertyFlags & properties) == properties)
                {
                    return i;
                }
            }

            Debug.Assert(false);

            return uint.MaxValue;
        }

        private unsafe void PopulateDebugMessengerCreateInfo(ref DebugUtilsMessengerCreateInfoEXT createInfo)
        {
            createInfo.SType = StructureType.DebugUtilsMessengerCreateInfoExt;
            createInfo.MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt |
                                         DebugUtilsMessageSeverityFlagsEXT.WarningBitExt |
                                         DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt;
            createInfo.MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt |
                                     DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt |
                                     DebugUtilsMessageTypeFlagsEXT.ValidationBitExt;

            createInfo.PfnUserCallback = (DebugUtilsMessengerCallbackFunctionEXT)DebugCallback;
        }

        private unsafe void SetupDebugMessenger()
        {
            //TryGetInstanceExtension equivilant to method CreateDebugUtilsMessengerEXT from original tutorial.
            if (!vk!.TryGetInstanceExtension(instance, out debugUtils)) return;

            DebugUtilsMessengerCreateInfoEXT createInfo = new();

            PopulateDebugMessengerCreateInfo(ref createInfo);

            if (debugUtils!.CreateDebugUtilsMessenger(instance, in createInfo, null, out debugMessenger) != Result.Success)
            {
                throw new Exception("failed to set up debug messenger!");
            }
        }

        private unsafe string[] GetRequiredExtensions(IVkSurface surface, bool bEnableValidationLayers)
        {
            var glfwExtensions = surface.GetRequiredExtensions(out var glfwExtensionCount);

            string[] extensions = SilkMarshal.PtrToStringArray((nint)glfwExtensions, (int)glfwExtensionCount);

            if (bEnableValidationLayers)
            {
                return extensions.Append(ExtDebugUtils.ExtensionName).ToArray();
            }

            return extensions;
        }
        private unsafe bool CheckValidationLayerSupport()
        {
            uint layerCount = 0;

            vk!.EnumerateInstanceLayerProperties(ref layerCount, null);

            var availableLayers = new LayerProperties[layerCount];

            fixed (LayerProperties* availableLayersPtr = availableLayers)
            {
                vk!.EnumerateInstanceLayerProperties(ref layerCount, availableLayersPtr);
            }

            var availableLayerNames = availableLayers.Select(layer => Marshal.PtrToStringAnsi((IntPtr)layer.LayerName)).ToHashSet();

            return validationLayers.All(availableLayerNames.Contains);
        }

        private unsafe uint DebugCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity, DebugUtilsMessageTypeFlagsEXT messageTypes, DebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData)
        {
            var message = $"validation layer:" + Marshal.PtrToStringAnsi((nint)pCallbackData->PMessage);

            Console.WriteLine(message);

            if(messageSeverity.HasFlag(DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt))
            {
                Debug.Assert(false, message);
            }

            return Vk.False;
        }

        private unsafe void CreateCommandPool()
        {
            var queueFamilyIndices = FindQueueFamilies(physicalDevice);

            CommandPoolCreateInfo poolInfo = new()
            {
                SType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = queueFamilyIndices.GraphicsFamily!.Value,
                Flags = CommandPoolCreateFlags.ResetCommandBufferBit
            };

            if (vk!.CreateCommandPool(_dev, ref poolInfo, null, out commandPool) != Result.Success)
            {
                throw new Exception("failed to create command pool!");
            }
        }

        private unsafe void CreateCommandBuffers()
        {
            commandBuffers = new CommandBuffer[MaxFramesInFlight];

            CommandBufferAllocateInfo allocInfo = new()
            {
                SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = commandPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = (uint)commandBuffers.Length,
            };

            fixed (CommandBuffer* commandBuffersPtr = commandBuffers)
            {
                if (vk!.AllocateCommandBuffers(_dev, ref allocInfo, commandBuffersPtr) != Result.Success)
                {
                    throw new Exception("failed to allocate command buffers!");
                }
            }
        }

        private unsafe void CreateSyncObjects()
        {
            imageAvailableSemaphores = new Semaphore[MaxFramesInFlight];
            renderFinishedSemaphores = new Semaphore[MaxFramesInFlight];
            inFlightFences = new Fence[MaxFramesInFlight];
            //imagesInFlight = new Fence[swapChainImages!.Length];

            SemaphoreCreateInfo semaphoreInfo = new()
            {
                SType = StructureType.SemaphoreCreateInfo,
            };

            FenceCreateInfo fenceInfo = new()
            {
                SType = StructureType.FenceCreateInfo,
                Flags = FenceCreateFlags.SignaledBit,
            };

            for (var i = 0; i < MaxFramesInFlight; i++)
            {
                if (vk!.CreateSemaphore(_dev, ref semaphoreInfo, null, out imageAvailableSemaphores[i]) != Result.Success ||
                    vk!.CreateSemaphore(_dev, ref semaphoreInfo, null, out renderFinishedSemaphores[i]) != Result.Success ||
                    vk!.CreateFence(_dev, ref fenceInfo, null, out inFlightFences[i]) != Result.Success)
                {
                    throw new Exception("failed to create synchronization objects for a frame!");
                }
            }
        }

        struct QueueFamilyIndices
        {
            public uint? GraphicsFamily { get; set; }
            public uint? PresentFamily { get; set; }
            public bool IsComplete()
            {
                return GraphicsFamily.HasValue && PresentFamily.HasValue;
            }
        }

        struct SwapChainSupportDetails
        {
            public SurfaceCapabilitiesKHR   Capabilities;
            public SurfaceFormatKHR[]       Formats;
            public PresentModeKHR[]         PresentModes;
        }

        public event EventHandler?          SwapchainDirty;
        public Device                       Device => _dev;

        private Device                       _dev;

        private RenderDeviceContext         renderDeviceContext;

        private Vk?                         vk;
        private Instance                    instance;
        private PhysicalDevice              physicalDevice;

        private KhrSurface?                 khrSurface;
        private SurfaceKHR                  surface;

        private uint                        surfaceWidth = 0;
        private uint                        surfaceHeight = 0;

        private KhrSwapchain?               khrSwapChain;
        private SwapchainKHR                swapChain;
        private Image[]?                    swapChainImages;
        private Format                      swapChainImageFormat;
        private Extent2D                    swapChainExtent;
        private ImageView[]?                swapChainImageViews;
        private Framebuffer[]?              swapChainFramebuffers;  
        
        private CommandPool                 commandPool;
        private CommandBuffer[]?            commandBuffers;
        private CommandBuffer               currentCommandBuffer;
        private CommandBuffer?              _uploadCommandBuffer;

        private Queue                       graphicsQueue;
        private Queue                       presentQueue;

        private PipelineLayout              pipelineLayout;
        private Pipeline                    graphicsPipeline;
        private RenderPass                  renderPass;

        private Semaphore[]?                imageAvailableSemaphores;
        private Semaphore[]?                renderFinishedSemaphores;
        private Fence[]?                    inFlightFences;
        private uint                        currentFrame = 0;

        private bool                        bEnableValidationLayers = false;
        private DebugUtilsMessengerEXT      debugMessenger;
        private ExtDebugUtils?              debugUtils;

        private uint                        MaxFramesInFlight = 0;

        private readonly string[]           validationLayers = ["VK_LAYER_KHRONOS_validation"];
        private readonly string[]           deviceExtensions = [KhrSwapchain.ExtensionName];
    }
}
