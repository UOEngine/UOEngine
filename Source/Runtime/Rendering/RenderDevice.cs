using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Semaphore = Silk.NET.Vulkan.Semaphore;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace UOEngine.Runtime.Rendering
{
    public class RenderDevice
    {
        public RenderDevice()
        {
            AddVertexFormat(EVertexFormat.R32G32SignedFloat, Format.R32G32Sfloat);
            AddVertexFormat(EVertexFormat.R32G32B32A32SignedFloat, Format.R32G32B32A32Sfloat);
        }

        public unsafe void Initialise(string[] extensions, bool bEnableValidationLayers)
        {
            this.bEnableValidationLayers = bEnableValidationLayers;

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

            if (bEnableValidationLayers)
            {
                extensions = extensions.Append(ExtDebugUtils.ExtensionName).ToArray();
            }

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

            PickPhysicalDevice();
            CreateLogicalDevice(bEnableValidationLayers);
            //CreateSwapChain(width, height);
            //CreateRenderPass();
            //CreateImageViews();
            //CreateFramebuffers();
            CreateCommandPool();
            CreateCommandBuffers();
            CreateSyncObjects();

            CreateDescriptorPool();
        }

        public unsafe void Shutdown()
        {
            WaitUntilIdle();

            //CleanupSwapchain();

            for (var i = 0; i < MaxFramesInFlight; i++)
            {
                vk!.DestroySemaphore(_dev, renderFinishedSemaphores![i], null);
                //vk.DestroySemaphore(_dev, imageAvailableSemaphores![i], null);
                vk.DestroyFence(_dev, inFlightFences![i], null);
            }

            vk!.DestroyCommandPool(_dev, commandPool, null);

            //foreach (var framebuffer in swapChainFramebuffers!)
            //{
            //    vk!.DestroyFramebuffer(_dev, framebuffer, null);
            //}

            Array.ForEach(_pipelineLayouts, pipelineLayout => vk.DestroyPipelineLayout(_dev, pipelineLayout, null));
            Array.ForEach(_graphicsPipelines, graphicsPipeline => vk.DestroyPipeline(_dev, graphicsPipeline, null));

            //foreach (var imageView in swapChainImageViews!)
            //{
            //    vk!.DestroyImageView(_dev, imageView, null);
            //}

            vk!.DestroyDevice(_dev, null);

            if (bEnableValidationLayers)
            {
                debugUtils!.DestroyDebugUtilsMessenger(instance, debugMessenger, null);
            }

            //khrSurface!.DestroySurface(instance, surface, null);
            vk!.DestroyInstance(instance, null);

            vk!.Dispose();
        }

        public int RegisterShader<T>() where T: Shader, new()
        {
            var shader = new T();

            int shaderHash = shader.GetHashCode();

            if (_shaders.ContainsKey(shaderHash))
            {
                Debug.Assert(false);
            }

            shader.Setup();

            int shaderId = _currentNumPipelines;

            //CreateGraphicsPipeline(shader);

            _shaders.Add(shaderHash, shader);

            return shaderId;
        }

        public Pipeline GetPipeline(int shaderId)
        {
            return _graphicsPipelines[shaderId];
        }

        public PipelineLayout GetPipelineLayout(int shaderId)
        {
            return _pipelineLayouts[shaderId];
        }

        public PipelineStateObjectDescription GetPipelineStateObjectDescription(int shaderId)
        {
            return _pipelineStateObjectDescriptions[shaderId];
        }

        public void SetSurfaceSize(uint width, uint height)
        {
            Console.WriteLine($"SetSurfaceSize: {width} {height}");
            //surfaceWidth = width; 
            //surfaceHeight = height;
        }
        public void OnFrameBegin()
        {
            ImmediateCommandList = _renderCommandLists![currentFrame];

            ImmediateCommandList.Reset();
            ImmediateCommandList.Begin();
        }

        public void SetupPresentQueue(KhrSurface surface, SurfaceKHR surfaceKHR)
        {
            for(uint i = 0; i < _queueFamilyProperties!.Length; i++)
            {
                surface.GetPhysicalDeviceSurfaceSupport(_physicalDevice, i, surfaceKHR, out var bSupportsPresent);

                if(bSupportsPresent)
                {
                    PresentQueueFamilyIndex = i;
                    break;
                }
            }

            vk!.GetDeviceQueue(_dev, PresentQueueFamilyIndex, 0, out _presentQueue);
        }

        public uint CreateRenderPass(ERenderTextureFormat format)
        {
            return 0;
        }

        //public unsafe void BeginRenderPass()
        //{
        //    ImmediateCommandList.BeginRenderPass(renderPass, _renderSwapChain.CurrentSwapChainFrameBuffer, _renderSwapChain.Extent);
        //}

        //public void EndRenderPass()
        //{
        //    ImmediateCommandList.EndRenderPass();
        //}

        public unsafe void Submit(RenderCommandList renderCommandList)
        {
            //var result = vk!.WaitForFences(_dev, 1, ref inFlightFences![currentFrame], true, ulong.MaxValue);

            //uint imageIndex = 0;
            //result = khrSwapChain!.AcquireNextImage(_dev, swapChain, ulong.MaxValue, imageAvailableSemaphores![currentFrame], default, ref imageIndex);

            //if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr)// || frameBufferResized)
            //{
            //    //frameBufferResized = false;

            //    RecreateSwapChain();

            //    return;
            //}
            //else if (result != Result.Success && result != Result.SuboptimalKhr)
            //{
            //    throw new Exception("failed to present swap chain image!");
            //}

            //result = vk!.ResetFences(_dev, 1, ref inFlightFences[currentFrame]);

            //SubmitInfo submitInfo = new()
            //{
            //    SType = StructureType.SubmitInfo,
            //};

            //var waitSemaphores = stackalloc[] { imageAvailableSemaphores[currentFrame] };
            //var waitStages = stackalloc[] { PipelineStageFlags.ColorAttachmentOutputBit };

            var buffer = renderCommandList.Handle;

            SubmitInfo submitInfo = new()
            { 
                SType = StructureType.SubmitInfo,
                CommandBufferCount = 1,
                PCommandBuffers = &buffer
            };

            //var signalSemaphores = stackalloc[] { renderFinishedSemaphores![currentFrame] };
            //submitInfo = submitInfo with
            //{
            //    SignalSemaphoreCount = 1,
            //    PSignalSemaphores = signalSemaphores,
            //};

            if (vk!.QueueSubmit(_graphicsQueue, 1, ref submitInfo, default) != Result.Success)
            {
                throw new Exception("failed to submit draw command buffer!");
            }

            //var swapChains = stackalloc[] { swapChain };

            //PresentInfoKHR presentInfo = new()
            //{
            //    SType = StructureType.PresentInfoKhr,

            //    WaitSemaphoreCount = 1,
            //    PWaitSemaphores = signalSemaphores,

            //    SwapchainCount = 1,
            //    PSwapchains = swapChains,

            //    PImageIndices = &imageIndex
            //};

            //result = khrSwapChain.QueuePresent(presentQueue, ref presentInfo);

            //currentFrame = (currentFrame + 1) % MaxFramesInFlight;

            //WaitUntilIdle();
        }

        public void WaitUntilIdle()
        {
            vk!.DeviceWaitIdle(_dev);
        }

        //public unsafe RenderCommandList BeginRecording()
        //{
        //    Debug.Assert(false);

        //    CommandBufferAllocateInfo allocInfo = new()
        //    {
        //        SType = StructureType.CommandBufferAllocateInfo,
        //        CommandPool = commandPool,
        //        Level = CommandBufferLevel.Primary,
        //        CommandBufferCount = 1,
        //    };

        //    if (vk!.AllocateCommandBuffers(_dev, ref allocInfo, out var commandBuffer) != Result.Success)
        //    {
        //        throw new Exception("failed to allocate command buffers!");
        //    }

        //    CommandBufferBeginInfo beginInfo = new()
        //    {
        //        SType = StructureType.CommandBufferBeginInfo,
        //        Flags = CommandBufferUsageFlags.OneTimeSubmitBit,
        //    };

        //    vk!.BeginCommandBuffer(commandBuffer, ref beginInfo);

        //    _uploadCommandBuffer = commandBuffer;

        //    return new RenderCommandList(commandBuffer, this);
        //}

        //public unsafe void EndRecording()
        //{
        //    Debug.Assert(_uploadCommandBuffer != null);

        //    var commandBuffer = _uploadCommandBuffer.Value;

        //    vk!.EndCommandBuffer(commandBuffer);

        //    SubmitInfo submitInfo = new()
        //    {
        //        SType = StructureType.SubmitInfo,
        //        CommandBufferCount = 1,
        //        PCommandBuffers = &commandBuffer,
        //    };

        //    vk!.QueueSubmit(graphicsQueue, 1, ref submitInfo, default);
        //    vk!.QueueWaitIdle(graphicsQueue);

        //    vk!.FreeCommandBuffers(_dev, commandPool, 1, ref commandBuffer);

        //    _uploadCommandBuffer = null;

        //}
        public RenderCommandList GetUploadCommandList()
        {
            if(_renderCommandListUpload!.State != ERenderCommandListState.Recording)
            {
                _renderCommandListUpload.Begin();
            }

            return _renderCommandListUpload;
        }

        public RenderTexture2D CreateTexture2D(RenderTexture2DDescription description)
        {
            return new RenderTexture2D(description, this);
        }

        public RenderBuffer CreateRenderBuffer(uint size, ERenderBufferType usageFlags)
        {
            var renderBuffer = new RenderBuffer(usageFlags, this);

            return renderBuffer;
        }

        public RenderBuffer CreateRenderBuffer<T>(T[] data, ERenderBufferType usageFlags)
        {
            var renderBuffer = new RenderBuffer(usageFlags, this);

            renderBuffer.CopyToDevice(new ReadOnlySpan<T>(data));

            return renderBuffer;
        }

        public unsafe void CreateBuffer(ulong size, BufferUsageFlags usage, MemoryPropertyFlags properties, out Buffer buffer, out DeviceMemory bufferMemory)
        {
            BufferCreateInfo bufferCreateInfo = new()
            {
                SType = StructureType.BufferCreateInfo,
                Size = size,
                Usage = usage,
                SharingMode = SharingMode.Exclusive
            };

            var vk = Vk.GetApi();

            var result = vk.CreateBuffer(_dev, ref bufferCreateInfo, null, out buffer);

            Debug.Assert(result == Result.Success);

            vk.GetBufferMemoryRequirements(_dev, buffer, out var memoryRequirements);

            MemoryAllocateInfo memoryAllocateInfo = new()
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = memoryRequirements.Size,
                MemoryTypeIndex = FindMemoryType(memoryRequirements.MemoryTypeBits, properties)
            };

            result = vk.AllocateMemory(_dev, ref memoryAllocateInfo, null, out bufferMemory);

            Debug.Assert(result == Result.Success);

            result = vk.BindBufferMemory(_dev, buffer, bufferMemory, 0);

            Debug.Assert(result == Result.Success);
        }
        public ImageView CreateImageView(Image image, Format format)
        {
            ImageViewCreateInfo imageViewCreateInfo = new()
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = image,
                ViewType = ImageViewType.Type2D,
                Format = format,
                Components =
                {
                    R = ComponentSwizzle.Identity,
                    G = ComponentSwizzle.Identity,
                    B = ComponentSwizzle.Identity,
                    A = ComponentSwizzle.Identity,
                },
                SubresourceRange = new()
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1
                }
            };

            ImageView imageView = default;

            unsafe
            {
                vk!.CreateImageView(_dev, ref imageViewCreateInfo, null, out imageView);
            }

            return imageView;

        }

        public Sampler CreateSampler()
        {
            vk!.GetPhysicalDeviceProperties(_physicalDevice, out PhysicalDeviceProperties properties);

            SamplerCreateInfo samplerInfo = new()
            {
                SType = StructureType.SamplerCreateInfo,
                MagFilter = Filter.Linear,
                MinFilter = Filter.Linear,
                AddressModeU = SamplerAddressMode.Repeat,
                AddressModeV = SamplerAddressMode.Repeat,
                AddressModeW = SamplerAddressMode.Repeat,
                AnisotropyEnable = false,
                //AnisotropyEnable = true,
                MaxAnisotropy = properties.Limits.MaxSamplerAnisotropy,
                BorderColor = BorderColor.IntOpaqueBlack,
                UnnormalizedCoordinates = false,
                CompareEnable = false,
                CompareOp = CompareOp.Always,
                MipmapMode = SamplerMipmapMode.Linear,
            };

            Sampler sampler = default;

            unsafe
            {
                if (vk.CreateSampler(_dev, ref samplerInfo, null, out sampler) != Result.Success)
                {
                    throw new Exception("failed to create texture sampler!");
                }
            }

            TextureSampler = sampler;

            return sampler;
        }

        public unsafe void AllocateDescriptorSets(DescriptorSetLayout[] descriptorSetLayouts, out DescriptorSet[] descriptorSets)
        {
            fixed (DescriptorSetLayout* desctorSetLayoutsPtr = &descriptorSetLayouts[0])
            {
                DescriptorSetAllocateInfo allocateInfo = new()
                {
                    SType = StructureType.DescriptorSetAllocateInfo,
                    DescriptorPool = _descriptorPools[0],
                    DescriptorSetCount = (uint)descriptorSetLayouts!.Length,
                    PSetLayouts = desctorSetLayoutsPtr,
                };

                descriptorSets = new DescriptorSet[descriptorSetLayouts.Length];

                fixed (DescriptorSet* descriptorSetsPtr = descriptorSets)
                {
                    var result = vk!.AllocateDescriptorSets(_dev, ref allocateInfo, descriptorSetsPtr);

                    if (result != Result.Success)
                    {
                        throw new Exception("failed to allocate descriptor sets!");
                    }
                }
            }
        }

        public unsafe CommandBuffer AllocateCommandBuffer()
        {
            CommandBufferAllocateInfo allocInfo = new()
            {
                SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = commandPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = 1,
            };

            if (vk!.AllocateCommandBuffers(Handle, ref allocInfo, out var buffer) != Result.Success)
            {
                throw new Exception("failed to allocate command buffers!");
            }

            return buffer;
        }

        private void PickPhysicalDevice()
        {
            var devices = vk!.GetPhysicalDevices(instance);

            foreach (var dev in devices)
            {
                    _physicalDevice = dev;
                    break;
            }

            if (_physicalDevice.Handle == 0)
            {
                throw new Exception("failed to find a suitable GPU!");
            }
        }

        private unsafe void CreateLogicalDevice(bool bEnableValidationLayers)
        {
            uint queueFamilyCount = 0;
            vk!.GetPhysicalDeviceQueueFamilyProperties(_physicalDevice, ref queueFamilyCount, null);

            _queueFamilyProperties = new QueueFamilyProperties[(int)queueFamilyCount];

            fixed (QueueFamilyProperties* queueFamiliesPtr = _queueFamilyProperties)
            {
                vk!.GetPhysicalDeviceQueueFamilyProperties(_physicalDevice, ref queueFamilyCount, queueFamiliesPtr);
            }

            for(uint i = 0; i < queueFamilyCount; i++)
            {
                var queueFamily = _queueFamilyProperties[i];

                if(queueFamily.QueueFlags.HasFlag(QueueFlags.GraphicsBit))
                {
                    GraphicsQueueFamilyIndex = i;
                }

                if (queueFamily.QueueFlags.HasFlag(QueueFlags.TransferBit))
                {
                    //TransferQueueFamilyIndex = i;
                }

            }

            const int numQueues = 1;

            var uniqueQueueFamilies = stackalloc uint[] { GraphicsQueueFamilyIndex, PresentQueueFamilyIndex };

            var queueCreateInfos = stackalloc DeviceQueueCreateInfo[numQueues];

            float queuePriority = 1.0f;
            for (int i = 0; i < numQueues; i++)
            {
                queueCreateInfos[i] = new()
                {
                    SType = StructureType.DeviceQueueCreateInfo,
                    QueueFamilyIndex = uniqueQueueFamilies[i],
                    QueueCount = numQueues,
                    PQueuePriorities = &queuePriority
                };
            }

            PhysicalDeviceFeatures deviceFeatures = new();

            DeviceCreateInfo createInfo = new()
            {
                SType = StructureType.DeviceCreateInfo,
                QueueCreateInfoCount = numQueues,
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

            if (vk!.CreateDevice(_physicalDevice, ref createInfo, null, out _dev) != Result.Success)
            {
                throw new Exception("failed to create logical device!");
            }

            Handle = _dev;

            vk.GetDeviceQueue(_dev, GraphicsQueueFamilyIndex, 0, out _graphicsQueue);
            //vk.GetDeviceQueue(_dev, PresentQueueFamilyIndex, 0, out _presentQueue);

            if (bEnableValidationLayers)
            {
                SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
            }

            SilkMarshal.Free((nint)createInfo.PpEnabledExtensionNames);
        }
        //private unsafe void CreateSurface(IVkSurface vkSurface)
        //{
        //    if (!vk!.TryGetInstanceExtension<KhrSurface>(instance, out khrSurface))
        //    {
        //        throw new NotSupportedException("KHR_surface extension not found.");
        //    }

        //    surface = vkSurface.Create<AllocationCallbacks>(instance.ToHandle(), null).ToSurface();
        //}

        private unsafe void CreateSwapChain(uint width, uint height)
        {
            //_renderSwapChain = new RenderSwapChain(this);

            //_renderSwapChain.Setup(width, height);

            //var swapChainSupport = QuerySwapChainSupport(physicalDevice);

            //var surfaceFormat = ChooseSwapSurfaceFormat(swapChainSupport.Formats);
            //var presentMode = ChoosePresentMode(swapChainSupport.PresentModes);
            //var extent = ChooseSwapExtent(swapChainSupport.Capabilities, width, height);

            //var imageCount = swapChainSupport.Capabilities.MinImageCount + 1;
            //if (swapChainSupport.Capabilities.MaxImageCount > 0 && imageCount > swapChainSupport.Capabilities.MaxImageCount)
            //{
            //    imageCount = swapChainSupport.Capabilities.MaxImageCount;
            //}

            //MaxFramesInFlight = imageCount;

            //SwapchainCreateInfoKHR createInfo = new()
            //{
            //    SType = StructureType.SwapchainCreateInfoKhr,
            //    Surface = surface,

            //    MinImageCount = imageCount,
            //    ImageFormat = surfaceFormat.Format,
            //    ImageColorSpace = surfaceFormat.ColorSpace,
            //    ImageExtent = extent,
            //    ImageArrayLayers = 1,
            //    ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            //};

            //var indices = FindQueueFamilies(physicalDevice);
            //var queueFamilyIndices = stackalloc[] { indices.GraphicsFamily!.Value, indices.PresentFamily!.Value };

            //if (indices.GraphicsFamily != indices.PresentFamily)
            //{
            //    createInfo = createInfo with
            //    {
            //        ImageSharingMode = SharingMode.Concurrent,
            //        QueueFamilyIndexCount = 2,
            //        PQueueFamilyIndices = queueFamilyIndices,
            //    };
            //}
            //else
            //{
            //    createInfo.ImageSharingMode = SharingMode.Exclusive;
            //}

            //createInfo = createInfo with
            //{
            //    PreTransform = swapChainSupport.Capabilities.CurrentTransform,
            //    CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
            //    PresentMode = presentMode,
            //    Clipped = true,

            //    OldSwapchain = default
            //};

            //if (!vk!.TryGetDeviceExtension(instance, _dev, out khrSwapChain))
            //{
            //    throw new NotSupportedException("VK_KHR_swapchain extension not found.");
            //}

            //if (khrSwapChain!.CreateSwapchain(_dev, ref createInfo, null, out swapChain) != Result.Success)
            //{
            //    throw new Exception("failed to create swap chain!");
            //}

            //khrSwapChain.GetSwapchainImages(_dev, swapChain, ref imageCount, null);
            //swapChainImages = new Image[imageCount];
            //fixed (Image* swapChainImagesPtr = swapChainImages)
            //{
            //    khrSwapChain.GetSwapchainImages(_dev, swapChain, ref imageCount, swapChainImagesPtr);
            //}

            //swapChainImageFormat = surfaceFormat.Format;
            //swapChainExtent = extent;
        }

        private unsafe void CreateDescriptorPool()
        {
            DescriptorPoolSize poolSize = new()
            {
                Type = DescriptorType.CombinedImageSampler,
                DescriptorCount = 3,
            };

            DescriptorPoolCreateInfo poolInfo = new()
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                PoolSizeCount = 1,
                PPoolSizes = &poolSize,
                MaxSets = 3,
            };

            fixed (DescriptorPool* descriptorPoolPtr = &_descriptorPools[0])
            {
                if (vk!.CreateDescriptorPool(_dev, ref poolInfo, null, descriptorPoolPtr) != Result.Success)
                {
                    throw new Exception("failed to create descriptor pool!");
                }

            }
        }

        private unsafe void CreateDescriptorSets()
        {
            //var layouts = new DescriptorSetLayout[swapChainImages!.Length];

            //Array.Fill(layouts, descriptorSetLayout);

            //fixed (DescriptorSetLayout* layoutsPtr = &_descriptorSetLayouts[0])
            //{
            //    DescriptorSetAllocateInfo allocateInfo = new()
            //    {
            //        SType = StructureType.DescriptorSetAllocateInfo,
            //        DescriptorPool = _descriptorPools[0],
            //        DescriptorSetCount = (uint)swapChainImages!.Length,
            //        PSetLayouts = layoutsPtr,
            //    };

            //    descriptorSets = new DescriptorSet[swapChainImages.Length];
            //    fixed (DescriptorSet* descriptorSetsPtr = &_des)
            //    {
            //        if (vk!.AllocateDescriptorSets(_dev, ref allocateInfo, descriptorSetsPtr) != Result.Success)
            //        {
            //            throw new Exception("failed to allocate descriptor sets!");
            //        }
            //    }
            //}

            //for (int i = 0; i < swapChainImages.Length; i++)
            //{
            //    DescriptorBufferInfo bufferInfo = new()
            //    {
            //        Buffer = uniformBuffers![i],
            //        Offset = 0,
            //        Range = (ulong)Unsafe.SizeOf<UniformBufferObject>(),

            //    };

            //    WriteDescriptorSet descriptorWrite = new()
            //    {
            //        SType = StructureType.WriteDescriptorSet,
            //        DstSet = descriptorSets[i],
            //        DstBinding = 0,
            //        DstArrayElement = 0,
            //        DescriptorType = DescriptorType.UniformBuffer,
            //        DescriptorCount = 1,
            //        PBufferInfo = &bufferInfo,
            //    };

            //    vk!.UpdateDescriptorSets(device, 1, descriptorWrite, 0, null);
            //}
        }

        private unsafe void CreateDescriptorSetLayout2(SetBindingDescription descriptor)
        {
            Debug.Assert(descriptor.ShaderStage != EShaderStage.None);
            Debug.Assert(descriptor.DescriptorType != EDescriptorType.None);

            int descriptorHash = descriptor.GetHashCode();

            if (_descriptorSetLayouts.ContainsKey(descriptorHash))
            {
                return;
            }

            DescriptorSetLayoutBinding layoutBinding = new()
            {
                Binding = descriptor.Binding,
                DescriptorCount = 1,
                DescriptorType = GetVulkanDescriptorType(descriptor.DescriptorType),
                PImmutableSamplers = null,
                StageFlags = descriptor.ShaderStage == EShaderStage.Vertex? ShaderStageFlags.VertexBit : ShaderStageFlags.FragmentBit,
                
            };

            DescriptorSetLayoutCreateInfo layoutInfo = new()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = 1,
                PBindings = &layoutBinding
            };

            DescriptorSetLayout descriptorSetLayout;

            if (vk!.CreateDescriptorSetLayout(_dev, ref layoutInfo, null, &descriptorSetLayout) != Result.Success)
            {
                throw new Exception("failed to create descriptor set layout!");
            }

            _descriptorSetLayouts.Add(descriptorHash, descriptorSetLayout);
        }

        //private unsafe void CleanupSwapchain()
        //{
        //    for (int i = 0; i < swapChainFramebuffers!.Length; i++)
        //    {
        //        vk!.DestroyFramebuffer(_dev, swapChainFramebuffers[i], null);
        //    }

        //    for (int i = 0; i < swapChainImageViews!.Length; i++)
        //    {
        //        vk!.DestroyImageView(_dev, swapChainImageViews[i], null);
        //    }

        //    khrSwapChain!.DestroySwapchain(_dev, swapChain, null);
        //}

        //private unsafe void CreateImageViews()
        //{
        //    swapChainImageViews = new ImageView[swapChainImages!.Length];

        //    for (int i = 0; i < swapChainImages.Length; i++)
        //    {
        //        ImageViewCreateInfo imageViewCreateInfo = new()
        //        {
        //            SType = StructureType.ImageViewCreateInfo,
        //            Image = swapChainImages[i],
        //            ViewType = ImageViewType.Type2D,
        //            Format = swapChainImageFormat,
        //            Components =
        //        {
        //            R = ComponentSwizzle.Identity,
        //            G = ComponentSwizzle.Identity,
        //            B = ComponentSwizzle.Identity,
        //            A = ComponentSwizzle.Identity,
        //        },
        //            SubresourceRange =
        //        {
        //            AspectMask = ImageAspectFlags.ColorBit,
        //            BaseMipLevel = 0,
        //            LevelCount = 1,
        //            BaseArrayLayer = 0,
        //            LayerCount = 1,
        //        }

        //        };

        //        if (vk!.CreateImageView(_dev, ref imageViewCreateInfo, null, out swapChainImageViews[i]) != Result.Success)
        //        {
        //            throw new Exception("failed to create image views!");
        //        }
        //    }
        //}

        private unsafe void CreateGraphicsPipeline(Shader shader, Extent2D extent)
        {
            int pipelineIndex = _currentNumPipelines;

            var vertexShaderModule = CreateShaderModule(shader.VertexByteCode);
            var fragmentShaderModule = CreateShaderModule(shader.FragmentByteCode);

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

            var vertexInputBindingDescriptions = stackalloc VertexInputBindingDescription[shader.VertexBindingDescriptions.Count];

            int i = 0;

            foreach (var bindingDescription in shader.VertexBindingDescriptions)
            {
                vertexInputBindingDescriptions[i++] = new()
                {
                    Binding = bindingDescription.Binding,
                    Stride = bindingDescription.Stride,
                    InputRate = VertexInputRate.Vertex
                };
            }

            var vertexAttributeDescriptions = stackalloc VertexInputAttributeDescription[shader.VertexAttributeDescriptions.Count];

            i = 0;

            foreach(var attribute in shader.VertexAttributeDescriptions)
            {
                vertexAttributeDescriptions[i++] = new()
                {
                    Binding = attribute.Binding,
                    Location = attribute.Location,
                    Format = _vertexFormats[(int)attribute.VertexFormat],
                    Offset = (uint)attribute.Offset
                };
            }

            PipelineVertexInputStateCreateInfo vertexInputInfo = new()
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                VertexBindingDescriptionCount = (uint)shader.VertexBindingDescriptions.Count,
                VertexAttributeDescriptionCount = (uint)shader.VertexAttributeDescriptions.Count
            };

            if(shader.VertexBindingDescriptions.Count > 0)
            {
                vertexInputInfo.PVertexBindingDescriptions = vertexInputBindingDescriptions;
            }

            if(shader.VertexAttributeDescriptions.Count > 0)
            {
                vertexInputInfo.PVertexAttributeDescriptions = vertexAttributeDescriptions;
            }

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
                Width = extent.Width,
                Height = extent.Height,
                MinDepth = 0,
                MaxDepth = 1,
            };

            Rect2D scissor = new()
            {
                Offset = { X = 0, Y = 0 },
                Extent = extent,
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

            int numDescriptors = shader.GetDescriptors().Count;
            var descriptorSetLayoutBindings = stackalloc DescriptorSetLayoutBinding[numDescriptors];

            uint numBindings = 0;

            foreach(var descriptorSet in shader.GetDescriptors())
            {
                DescriptorSetLayoutBinding layoutBinding = new()
                {
                    Binding = descriptorSet.Binding,
                    DescriptorCount = 1,
                    DescriptorType = GetVulkanDescriptorType(descriptorSet.DescriptorType),
                    PImmutableSamplers = null,
                    StageFlags = descriptorSet.ShaderStage == EShaderStage.Vertex ? ShaderStageFlags.VertexBit : ShaderStageFlags.FragmentBit,
                };

                descriptorSetLayoutBindings[numBindings] = layoutBinding;

                numBindings++;
            }

            DescriptorSetLayoutCreateInfo layoutInfo = new()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = (uint)numDescriptors,
                PBindings = descriptorSetLayoutBindings
            };

            PipelineLayout pipelineLayout;

            PipelineLayoutCreateInfo pipelineLayoutInfo = new()
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                PushConstantRangeCount = 0,
                SetLayoutCount = (uint)numDescriptors,
            };

            var descriptorSetLayouts = new DescriptorSetLayout[numDescriptors];

            if (numDescriptors > 0)
            {
                fixed (DescriptorSetLayout* descriptorSetLayoutsPtr = &descriptorSetLayouts[0])
                {
                    if (vk!.CreateDescriptorSetLayout(_dev, ref layoutInfo, null, descriptorSetLayoutsPtr) != Result.Success)
                    {
                        throw new Exception("failed to create descriptor set layout!");
                    }

                    pipelineLayoutInfo.PSetLayouts = descriptorSetLayoutsPtr;

                    if (vk!.CreatePipelineLayout(_dev, ref pipelineLayoutInfo, null, out pipelineLayout) != Result.Success)
                    {
                        throw new Exception("failed to create pipeline layout!");
                    }
                }
            }
            else
            {
                if (vk!.CreatePipelineLayout(_dev, ref pipelineLayoutInfo, null, out pipelineLayout) != Result.Success)
                {
                    throw new Exception("failed to create pipeline layout!");
                }
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
                RenderPass = renderPass,
                Layout = pipelineLayout,
                PDynamicState = &dynamicStateCreateInfo,
                Subpass = 0,
                BasePipelineHandle = default
            };

            if (vk!.CreateGraphicsPipelines(_dev, default, 1, ref pipelineInfo, null, out var graphicsPipeline) != Result.Success)
            {
                throw new Exception("failed to create graphics pipeline!");
            }

            vk!.DestroyShaderModule(_dev, vertexShaderModule, null);
            vk!.DestroyShaderModule(_dev, fragmentShaderModule, null);

            SilkMarshal.Free((nint)vertShaderStageInfo.PName);
            SilkMarshal.Free((nint)fragShaderStageInfo.PName);

            _pipelineLayouts[pipelineIndex] = pipelineLayout;
            _graphicsPipelines[pipelineIndex] = graphicsPipeline;

            PipelineStateObjectDescription p = new PipelineStateObjectDescription(graphicsPipeline, pipelineLayout, descriptorSetLayouts, [.. shader.GetDescriptors()]);

            _pipelineStateObjectDescriptions[pipelineIndex] = p;

            _currentNumPipelines++;
        }

        private unsafe void CreateRenderPass(Format format)
        {
            AttachmentDescription colorAttachment = new()
            {
                Format = format,
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

        private unsafe void CreateFramebuffers(ImageView[] imageViews, uint width, uint height)
        {
            var framebuffers = new Framebuffer[imageViews.Length];

            for (int i = 0; i < imageViews.Length; i++)
            {
                var attachment = imageViews[i];

                FramebufferCreateInfo framebufferInfo = new()
                {
                    SType = StructureType.FramebufferCreateInfo,
                    RenderPass = renderPass,
                    AttachmentCount = 1,
                    PAttachments = &attachment,
                    Width = width,
                    Height = height,
                    Layers = 1,
                };

                if (vk!.CreateFramebuffer(_dev, ref framebufferInfo, null, out framebuffers[i]) != Result.Success)
                {
                    throw new Exception("failed to create framebuffer!");
                }
            }
        }

        private unsafe ShaderModule CreateShaderModule(ReadOnlySpan<byte> code)
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

        //private bool IsDeviceSuitable(PhysicalDevice device)
        //{
        //    vk!.GetPhysicalDeviceFeatures(device, out var supportedFeatures);

        //    if(supportedFeatures.SamplerAnisotropy == false)
        //    {
        //        return false;
        //    }

        //    var indices = FindQueueFamilies(device);

        //    return indices.IsComplete();
        //}

        //private unsafe QueueFamilyIndices FindQueueFamilies(PhysicalDevice device)
        //{
        //    var indices = new QueueFamilyIndices();

        //    uint queueFamilityCount = 0;
        //    vk!.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilityCount, null);

        //    var queueFamilies = new QueueFamilyProperties[queueFamilityCount];
        //    fixed (QueueFamilyProperties* queueFamiliesPtr = queueFamilies)
        //    {
        //        vk!.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilityCount, queueFamiliesPtr);
        //    }

        //    uint i = 0;

        //    foreach (var queueFamily in queueFamilies)
        //    {
        //        if (queueFamily.QueueFlags.HasFlag(QueueFlags.GraphicsBit))
        //        {
        //            indices.GraphicsFamily = i;
        //        }

        //        khrSurface!.GetPhysicalDeviceSurfaceSupport(device, i, surface, out var presentSupport);

        //        if (presentSupport)
        //        {
        //            indices.PresentFamily = i;
        //        }

        //        if (indices.IsComplete())
        //        {
        //            break;
        //        }

        //        i++;
        //    }

        //    return indices;
        //}

        public uint FindMemoryType(uint typeFilter, MemoryPropertyFlags properties)
        {
            vk!.GetPhysicalDeviceMemoryProperties(_physicalDevice, out var memoryProperties);

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

        //private unsafe string[] GetRequiredExtensions(IVkSurface surface, bool bEnableValidationLayers)
        //{
        //    var glfwExtensions = surface.GetRequiredExtensions(out var glfwExtensionCount);

        //    string[] extensions = SilkMarshal.PtrToStringArray((nint)glfwExtensions, (int)glfwExtensionCount);

        //    if (bEnableValidationLayers)
        //    {
        //        return extensions.Append(ExtDebugUtils.ExtensionName).ToArray();
        //    }

        //    return extensions;
        //}
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
            CommandPoolCreateInfo poolInfo = new()
            {
                SType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = GraphicsQueueFamilyIndex,
                Flags = CommandPoolCreateFlags.ResetCommandBufferBit
            };

            if (vk!.CreateCommandPool(_dev, ref poolInfo, null, out commandPool) != Result.Success)
            {
                throw new Exception("failed to create command pool!");
            }
        }

        private unsafe void CreateCommandBuffers()
        {
            const int maxFramesInFlight = 3;

            _renderCommandLists = new RenderCommandListImmediate[maxFramesInFlight];

               // Ignoring frames in flight for now.
            ImmediateCommandList = new RenderCommandListImmediate(this);

            _renderCommandListUpload = new RenderCommandList(this);
        }

        private unsafe void CreateSyncObjects()
        {
            //imageAvailableSemaphores = new Semaphore[MaxFramesInFlight];
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
                if (vk!.CreateSemaphore(_dev, ref semaphoreInfo, null, out renderFinishedSemaphores[i]) != Result.Success ||
                    vk!.CreateFence(_dev, ref fenceInfo, null, out inFlightFences[i]) != Result.Success)
                {
                    throw new Exception("failed to create synchronization objects for a frame!");
                }
            }
        }

        private DescriptorType GetVulkanDescriptorType(EDescriptorType descriptorType)
        {
            switch (descriptorType)
            {
                case EDescriptorType.CombinedSampler:
                    return DescriptorType.CombinedImageSampler;

                case EDescriptorType.UniformBuffer:
                    return DescriptorType.UniformBuffer;
            }

            throw new Exception("Can not convert to vulkan descriptor type");
        }

        private void AddVertexFormat(EVertexFormat vertexFormat, Format vulkanFormat)
        {
            _vertexFormats[(int)vertexFormat] = vulkanFormat;
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

        //public event EventHandler?                      SwapchainDirty;
        public Device                                   Device => _dev;
        public Device                                   Handle { get; private set; }
        public PhysicalDevice                           PhysicalHandle => _physicalDevice;

        public RenderQueue                              GraphicsQueue { get; private set; }
        public RenderQueue                              PresentQueue { get; private set; }      

        public Instance                                 Instance => instance;

        //public CommandBuffer                            CurrentCommandBuffer { private get; private set; }
        public RenderCommandListImmediate?              ImmediateCommandList { get; private set; }

        public Sampler                                  TextureSampler { get; private set; }

        public uint                                     GraphicsQueueFamilyIndex { get; private set; } = uint.MaxValue;
        public uint                                     PresentQueueFamilyIndex { get; private set; } = uint.MaxValue;

        private Device                                  _dev;

        //private RenderDeviceContext                     renderDeviceContext;

        private Vk?                                     vk;
        private Instance                                instance;
        private PhysicalDevice                          _physicalDevice;

        private QueueFamilyProperties[]?                _queueFamilyProperties;

        //private KhrSurface?                             khrSurface;
        //private SurfaceKHR                                surface;

        //private uint                                    surfaceWidth = 0;
        //private uint                                    surfaceHeight = 0;

        //private KhrSwapchain?                           khrSwapChain;
        //private SwapchainKHR                            swapChain;
        //private Image[]?                                swapChainImages;
        //private Format                                  swapChainImageFormat;
        //private Extent2D                                swapChainExtent;
        //private ImageView[]?                            swapChainImageViews;
        //private Framebuffer[]?                          swapChainFramebuffers;
        //
        //private RenderSwapChain                         _renderSwapChain;

        private CommandPool                             commandPool;
        //private CommandBuffer[]?                        commandBuffers;
        private RenderCommandListImmediate[]?           _renderCommandLists;
        private RenderCommandList?                      _renderCommandListUpload;

        private Queue                                   _graphicsQueue;

        private Queue                                   _presentQueue;

        const int                                       MaxPipelines = 64;

        private int                                     _currentNumPipelines = 0;
        private PipelineLayout[]                        _pipelineLayouts = new PipelineLayout[MaxPipelines];
        private Pipeline[]                              _graphicsPipelines = new Pipeline[MaxPipelines];
        private PipelineStateObjectDescription[]        _pipelineStateObjectDescriptions = new PipelineStateObjectDescription[MaxPipelines];

        private RenderPass                              renderPass;

        private Semaphore[]?                            renderFinishedSemaphores;
        private Fence[]?                                inFlightFences;
        private uint                                    currentFrame = 0;

        private bool                                    bEnableValidationLayers = false;
        private DebugUtilsMessengerEXT                  debugMessenger;
        private ExtDebugUtils?                          debugUtils;

        private uint                                    MaxFramesInFlight = 0;

        private readonly string[]                       validationLayers = ["VK_LAYER_KHRONOS_validation"];
        private readonly string[]                       deviceExtensions = [KhrSwapchain.ExtensionName];

        private Dictionary<int, DescriptorSetLayout>    _descriptorSetLayouts = [];
        private DescriptorPool[]                        _descriptorPools = new DescriptorPool[1];
        
        private Dictionary<int, Shader>                 _shaders = [];

        private Format[]                                _vertexFormats = new Format[(int)Format.Astc12x12SrgbBlock];
    }
}
