using System.Diagnostics;
using System.Runtime.InteropServices;

using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Semaphore = Silk.NET.Vulkan.Semaphore;
using Buffer = Silk.NET.Vulkan.Buffer;
using UOEngine.Runtime.Rendering.Resources;

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
            CreateCommandPool();
            CreateCommandBuffers();

            CreateDescriptorPool();

            CreateSampler();
        }

        public unsafe void Shutdown()
        {
            WaitUntilIdle();

            vk!.DestroyCommandPool(_dev, commandPool, null);

            Array.ForEach(_pipelineLayouts, pipelineLayout => vk.DestroyPipelineLayout(_dev, pipelineLayout, null));
            Array.ForEach(_graphicsPipelines, graphicsPipeline => vk.DestroyPipeline(_dev, graphicsPipeline, null));

            vk!.DestroyDevice(_dev, null);

            if (bEnableValidationLayers)
            {
                debugUtils!.DestroyDebugUtilsMessenger(instance, debugMessenger, null);
            }

            vk!.DestroyInstance(instance, null);

            vk!.Dispose();
        }

        public T RegisterShader<T>() where T: Shader, new()
        {
            var shader = new T();

            int shaderHash = shader.GetHashCode();

            if (_shaders.ContainsKey(shaderHash))
            {
                return shader;
            }

            _shaders.Add(shaderHash, shader);

            shader.Setup();

            return shader;
        }

        public Pipeline GetPipeline(int shaderId)
        {
            return _graphicsPipelines[shaderId];
        }

        public PipelineLayout GetPipelineLayout(int shaderId)
        {
            return _pipelineLayouts[shaderId];
        }

        public PipelineStateObjectDescription GetOrCreatePipelineStateObject(Shader shader, RenderPass renderPass)
        {
            int hash = HashCode.Combine(shader.GetHashCode(), renderPass.GetHashCode());

            if(_pipelineStateObjectDescriptions.TryGetValue(hash, out var pipelineStateObjectDescription))
            {
                return pipelineStateObjectDescription;
            }

            return CreateGraphicsPipeline(shader, renderPass);
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

        public void SubmitAndFlush()
        {
            if(ImmediateContext!.CommandBufferManager.HasActiveCommandBuffer)
            {
                ImmediateContext!.CommandBufferManager.SubmitActive();
            }

            if(ImmediateContext!.CommandBufferManager.HasActiveUploadBuffer)
            {
                ImmediateContext!.CommandBufferManager.SubmitUploadBuffer();
            }

            ImmediateContext!.CommandBufferManager.PrepareNewActiveCommandBuffer();
        }

        public unsafe void Submit(RenderCommandBuffer commandBuffer)
        {
            var buffer = commandBuffer.Handle;

            SubmitInfo submitInfo = new()
            { 
                SType = StructureType.SubmitInfo,
                CommandBufferCount = 1,
                PCommandBuffers = &buffer
            };

            if(commandBuffer.WaitSemaphore != null)
            {
                var waitSemaphores = stackalloc[] { commandBuffer.WaitSemaphore.Value };
                var waitStages = stackalloc[] { commandBuffer.WaitFlags };

                submitInfo = submitInfo with
                {
                    PWaitSemaphores = waitSemaphores,
                    WaitSemaphoreCount = 1,
                    PWaitDstStageMask = waitStages
                };
            }

            if(commandBuffer.IsUploadBuffer == false)
            {
                var semaphores = stackalloc[] { commandBuffer.ExecutionComplete };

                submitInfo = submitInfo with
                {
                    SignalSemaphoreCount = 1,
                    PSignalSemaphores = semaphores,
                };
            }

            if (vk!.QueueSubmit(_graphicsQueue, 1, ref submitInfo, default) != Result.Success)
            {
                throw new Exception("failed to submit draw command buffer!");
            }
        }

        public void WaitUntilIdle()
        {
            vk!.DeviceWaitIdle(_dev);
        }

        public bool IsFenceSignaled(Fence fence)
        {
            Result result = vk!.GetFenceStatus(_dev, fence);

            switch (result)
            {
                case Result.Success:
                    return true;

                case Result.NotReady:
                    return false;

                default:
                    throw new Exception("Unknown state for fence.");
            }
        }

        public RenderTexture2D CreateTexture2D(RenderTexture2DDescription description)
        {
            return new RenderTexture2D(this, description);
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

        public unsafe Fence CreateFence()
        {
            FenceCreateInfo fenceInfo = new()
            {
                SType = StructureType.FenceCreateInfo,
                Flags = FenceCreateFlags.SignaledBit,
            };

            if(vk!.CreateFence(_dev, ref fenceInfo, null, out var fence) != Result.Success)
            {
                throw new Exception("failed to create synchronization objects for a frame!");
            }

            return fence;
        }

        public void WaitForFence(Fence fence)
        {
            Debug.Assert(fence.Handle != 0);
            
            vk!.WaitForFences(_dev, 1, ref fence, true, ulong.MaxValue);
            vk.ResetFences(_dev, 1, ref fence);
        }

        public unsafe Semaphore CreateSemaphore()
        {
            SemaphoreCreateInfo semaphoreInfo = new()
            {
                SType = StructureType.SemaphoreCreateInfo,
            };

            if (vk!.CreateSemaphore(_dev, ref semaphoreInfo, null, out var semaphore) != Result.Success)
            {
                throw new Exception("failed to create synchronization objects for a frame!");
            }

            return semaphore;
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

        public void RecycleDescriptorSets()
        {
            Vulkan.VkResetDescriptorPool(_dev, _descriptorPools[0]);
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

        private unsafe void PickPhysicalDevice()
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

        private unsafe PipelineStateObjectDescription CreateGraphicsPipeline(Shader shader, RenderPass renderPass)
        {
            ShaderResource shaderResource = new ShaderResource(shader.VertexByteCode, shader.FragmentByteCode);

            shaderResource.Generate();

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

            foreach (var attribute in shader.VertexAttributeDescriptions)
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
                Width = 0,
                Height = 0,
                MinDepth = 0,
                MaxDepth = 1,
            };

            Rect2D scissor = new()
            {
                Offset = { X = 0, Y = 0 },
                Extent = {Width = 0, Height = 0},
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

            int numDescriptors = shaderResource.DescriptorSetLayouts.Count;
            var descriptorSetLayoutBindings = stackalloc DescriptorSetLayoutBinding[numDescriptors];

            uint numBindings = 0;

            foreach(var descriptorSetInfo in shaderResource.DescriptorSetLayouts)
            {
                descriptorSetLayoutBindings[numBindings] = descriptorSetInfo;

                numBindings++;
            }

            DescriptorSetLayoutCreateInfo layoutInfo = new()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = (uint)shaderResource.DescriptorSetLayouts.Count,
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

            int hash = HashCode.Combine(shader, renderPass);

            PipelineStateObjectDescription pipelineStateObjectDescription = new(graphicsPipeline, descriptorSetLayouts, shaderResource.DescriptorSetLayouts.ToArray(), pipelineLayout);

            _pipelineStateObjectDescriptions.Add(hash, pipelineStateObjectDescription);

            return pipelineStateObjectDescription;
        }

        public RenderPass GetOrCreateRenderPass(in RenderPassInfo renderPassInfo)
        {
            if(_renderPasses.TryGetValue(renderPassInfo.GetHashCode(), out var renderPass))
            {
                return renderPass;
            }

            return CreateRenderPass(renderPassInfo);
        }

        public unsafe RenderPass CreateRenderPass(in RenderPassInfo renderPassInfo)
        {
            Console.WriteLine("Creating render pass");

            AttachmentDescription colorAttachment = new()
            {
                Format = RenderCommon.TextureFormatToVulkanFormat(renderPassInfo.TextureFormat),
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

            RenderPassCreateInfo renderPassCreateInfo = new()
            {
                SType = StructureType.RenderPassCreateInfo,
                AttachmentCount = 1,
                PAttachments = &colorAttachment,
                SubpassCount = 1,
                PSubpasses = &subpass,
                DependencyCount = 1,
                PDependencies = &dependency
            };

            if (vk!.CreateRenderPass(_dev, ref renderPassCreateInfo, null, out var renderPass) != Result.Success)
            {
                throw new Exception("failed to create render pass!");
            }

            _renderPasses.Add(renderPassInfo.GetHashCode(), renderPass);

            return renderPass;
        }

        public RenderFramebuffer GetOrCreateFrameBuffer(in RenderTargetInfo renderTargetInfo, RenderPass renderPass)
        {
            int hashCode = HashCode.Combine(renderTargetInfo.GetHashCode(), renderPass.GetHashCode());

            if (_frameBuffers.TryGetValue(hashCode, out RenderFramebuffer? framebuffer))
            {
                return framebuffer;
            }

            framebuffer = new RenderFramebuffer(this, renderTargetInfo, renderPass);

            _frameBuffers.Add(hashCode, framebuffer);

            return framebuffer;
        }

        public unsafe void DestroyFramebuffer(Framebuffer framebuffer)
        {
            vk!.DestroyFramebuffer(_dev, framebuffer, null);
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
               // Ignoring frames in flight for now.
            ImmediateContext = new RenderCommandListContextImmediate(this);

            _renderCommandListUpload = new RenderCommandListContext(this);
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

        public Device                                   Device => _dev;
        public Device                                   Handle { get; private set; }
        public PhysicalDevice                           PhysicalHandle => _physicalDevice;

        public RenderCommandListContextImmediate?       ImmediateContext { get; private set; }

        public RenderQueue                              GraphicsQueue { get; private set; }
        public Queue                                    PresentQueue => _presentQueue;      

        public Instance                                 Instance => instance;
        public Sampler                                  TextureSampler { get; private set; }

        public uint                                     GraphicsQueueFamilyIndex { get; private set; } = uint.MaxValue;
        public uint                                     PresentQueueFamilyIndex { get; private set; } = uint.MaxValue;

        private Device                                  _dev;

        private Vk?                                     vk;
        private Instance                                instance;
        private PhysicalDevice                          _physicalDevice;

        private QueueFamilyProperties[]?                _queueFamilyProperties;

        private CommandPool                             commandPool;

        private RenderCommandListContext?               _renderCommandListUpload;

        private Queue                                   _graphicsQueue;

        private Queue                                   _presentQueue;

        const int                                       MaxPipelines = 64;

        private PipelineLayout[]                        _pipelineLayouts = new PipelineLayout[MaxPipelines];
        private Pipeline[]                              _graphicsPipelines = new Pipeline[MaxPipelines];

        private Dictionary<int, PipelineStateObjectDescription>        
                                                        _pipelineStateObjectDescriptions = [];

        private Dictionary<int, RenderPass>            _renderPasses = [];

        private Dictionary<int, RenderFramebuffer>     _frameBuffers = [];

        private bool                                    bEnableValidationLayers = false;
        private DebugUtilsMessengerEXT                  debugMessenger;
        private ExtDebugUtils?                          debugUtils;

        private readonly string[]                       validationLayers = ["VK_LAYER_KHRONOS_validation"];
        private readonly string[]                       deviceExtensions = [KhrSwapchain.ExtensionName];

        private Dictionary<int, DescriptorSetLayout>    _descriptorSetLayouts = [];
        private DescriptorPool[]                        _descriptorPools = new DescriptorPool[1];
        
        private Dictionary<int, Shader>                 _shaders = [];

        private Format[]                                _vertexFormats = new Format[(int)Format.Astc12x12SrgbBlock];
    }
}
