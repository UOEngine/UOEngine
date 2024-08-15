using System.Diagnostics;

using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;
using VkSemaphore = Silk.NET.Vulkan.Semaphore;


namespace UOEngine.Runtime.Rendering
{
    public enum ERenderCommandListState
    { 
        Invalid,
        NotAllocated,
        Ready,
        Reset,
        Recording,
        Ended,
        Submitted,
        Finished
    }

    public class RenderCommandList: IDisposable
    {
        public RenderCommandList(RenderDevice renderDevice)
        {
            _renderDevice = renderDevice;

            _vk = Vk.GetApi();

            State = ERenderCommandListState.NotAllocated;

            AllocateCommandBuffer();
        }

        public void Dispose()
        {
            Debug.Assert(IsImmediate == false);

            SubmitAndWaitUntilGPUIdle();
        }

        public void Reset()
        {
            Debug.Assert(State == ERenderCommandListState.Finished);

            _vk.ResetCommandBuffer(_commandBuffer, 0);
            State = ERenderCommandListState.Reset;
        }

        public void Begin()
        {
            CommandBufferBeginInfo beginInfo = new()
            {
                SType = StructureType.CommandBufferBeginInfo,
            };

            if(_vk.BeginCommandBuffer(_commandBuffer, ref beginInfo) != Result.Success)
            {
                throw new Exception("failed to begin command buffer!");
            }

            State = ERenderCommandListState.Recording;
        }

        public void End()
        {
            Debug.Assert(_bInRenderPass == false);
            Debug.Assert(State == ERenderCommandListState.Recording);

            _vk.EndCommandBuffer(_commandBuffer);

            State = ERenderCommandListState.Ended;
        }

        public unsafe void BeginRenderPass(RenderPass renderPass, Framebuffer framebuffer, Extent2D extent)
        {
            Debug.Assert(_bInRenderPass == false);

            RenderPassBeginInfo renderPassInfo = new()
            {
                SType = StructureType.RenderPassBeginInfo,
                RenderPass = renderPass,
                Framebuffer = framebuffer,
                RenderArea =
                {
                    Offset = { X = 0, Y = 0 },
                    Extent = extent,
                }
            };

            ClearValue clearColor = new()
            {
                Color = new() { Float32_0 = 0, Float32_1 = 0, Float32_2 = 0, Float32_3 = 1 },
            };

            renderPassInfo.ClearValueCount = 1;
            renderPassInfo.PClearValues = &clearColor;

            _vk.CmdBeginRenderPass(_commandBuffer, &renderPassInfo, SubpassContents.Inline);

            Viewport viewport = new()
            {
                X = 0,
                Y = 0,
                Width = extent.Width,
                Height = extent.Height,
                MinDepth = 0.0f,
                MaxDepth = 1.0f
            };

            _vk.CmdSetViewport(_commandBuffer, 0, 1, ref viewport);

            Rect2D scissor = new()
            {
                Offset = new() { X = 0, Y = 0 },
                Extent = extent
            };

            _vk.CmdSetScissor(_commandBuffer, 0, 1, ref scissor);

            _bInRenderPass = true;
        }

        public void EndRenderPass()
        {
            Debug.Assert(_bInRenderPass);

            _vk!.CmdEndRenderPass(_commandBuffer);

            _bInRenderPass = false;
        }

        public void CopyBuffer(Buffer srcBuffer, Buffer destBuffer, BufferCopy copyRegion)
        {
            _vk.CmdCopyBuffer(_commandBuffer, srcBuffer, destBuffer, 1, ref copyRegion);

        }
        public void CopyBufferToImage(Buffer buffer, Image image, ImageLayout imageLayout, uint regionCount, BufferImageCopy bufferImageCopy)
        {
            _vk.CmdCopyBufferToImage(_commandBuffer, buffer, image, ImageLayout.TransferDstOptimal, regionCount, ref bufferImageCopy);
        }

        public void BindIndexBuffer(RenderBuffer buffer)
        {
            _vk.CmdBindIndexBuffer(_commandBuffer, buffer.DeviceBuffer, 0, IndexType.Uint16);
        }

        public unsafe void BindVertexBuffer(RenderBuffer buffer)
        {
            Debug.Assert(false);
            //fixed (Buffer* vertexBuffersPtr = buffer.DeviceBuffer)
            //{
            //    _vk.CmdBindVertexBuffers(_commandBuffer, 0, 0, vertexBuffersPtr, 0);
            //}
        }

        public void SetTexture(RenderTexture2D texture, uint slot)
        {
            _textures[slot] = texture;
        }
        public void SetImageLayout()
        {

        }

        public void PipelineBarrier(PipelineStageFlags sourceStage, PipelineStageFlags destinationStage, ImageMemoryBarrier imageMemoryBarrier)
        {
            unsafe
            {
                _vk.CmdPipelineBarrier(_commandBuffer, sourceStage, destinationStage, 0, 0, null, 0, null, 1, ref imageMemoryBarrier);
            }
        }

        public void DrawIndexed(uint indexCount, uint instanceCount)
        {
            _vk.CmdDrawIndexed(_commandBuffer, indexCount, instanceCount, 0, 0, 0);
        }

        public void BindShader(Shader shader)
        {
            BindShader(shader.GetHashCode());
        }

        public unsafe void BindShader(int shaderId)
        {
            _currentShaderId = shaderId;

            PipelineStateObjectDescription pso = _renderDevice.GetPipelineStateObjectDescription(_currentShaderId);

            _vk.CmdBindPipeline(_commandBuffer, PipelineBindPoint.Graphics, pso.PSO);

            _renderDevice.AllocateDescriptorSets(pso.DescriptorSetLayouts, out var descriptorSets);

            const int maxDescriptorImageInfos = 4;
            int numDescriptorImageInfos = 0;
            var descriptorImageInfos = stackalloc DescriptorImageInfo[maxDescriptorImageInfos];

            Span<WriteDescriptorSet> descriptorWrites = stackalloc WriteDescriptorSet[descriptorSets.Length];

            int numUpdated = 0;

            foreach (var bindingDescription in pso.BindingDescriptions)
            {
                WriteDescriptorSet writeDescriptorSet = new WriteDescriptorSet();

                writeDescriptorSet.DstBinding = bindingDescription.Binding;
                writeDescriptorSet.SType = StructureType.WriteDescriptorSet;
                writeDescriptorSet.DescriptorCount = 1;
                writeDescriptorSet.DstSet = descriptorSets[numUpdated];

                switch (bindingDescription.DescriptorType)
                {
                    case EDescriptorType.CombinedSampler:
                        {
                            DescriptorImageInfo imageInfo = descriptorImageInfos[numDescriptorImageInfos];

                            imageInfo.ImageLayout = ImageLayout.ShaderReadOnlyOptimal;

                            imageInfo.ImageView = _textures[bindingDescription.Binding].ShaderResourceView;
                            imageInfo.Sampler = _renderDevice.TextureSampler;

                            descriptorImageInfos[numDescriptorImageInfos] = imageInfo;

                            writeDescriptorSet.DescriptorType = DescriptorType.CombinedImageSampler;
                            writeDescriptorSet.PImageInfo = &descriptorImageInfos[numDescriptorImageInfos];

                            numDescriptorImageInfos++;
                        }
                        break;

                    default:
                        {
                            Debug.Assert(false);
                        }
                        break;
                }

                descriptorWrites[numUpdated++] = writeDescriptorSet;
            }

            _vk.UpdateDescriptorSets(_renderDevice.Device, (uint)numUpdated, descriptorWrites, 0, []);

            }

        public void SubmitAndWaitUntilGPUIdle()
        {
            Submit();

            _renderDevice.WaitUntilIdle();
        }

        public void Submit()
        {
            Debug.Assert(State == ERenderCommandListState.Recording);

            _renderDevice.Submit(this);

            State = ERenderCommandListState.Submitted;
        }

        public unsafe void AllocateCommandBuffer()
        {
            _commandBuffer = _renderDevice.AllocateCommandBuffer();

            RenderingCompletedSemaphore = _renderDevice.CreateSemaphore();

            State = ERenderCommandListState.Ready;
        }

        public ERenderCommandListState  State { get; private set; } = ERenderCommandListState.Invalid;
        public CommandBuffer            Handle => _commandBuffer;
        public bool                     IsImmediate { get; protected set; } = false;

        public VkSemaphore              RenderingCompletedSemaphore { get; private set; }
        public VkSemaphore              UploadCompletedSemaphore { get; private set; }

        // Do not start executing this command list until this semaphore is signaled.
        public VkSemaphore?           WaitSemaphore { get; set; }
        public PipelineStageFlags       WaitFlags { get; set; }

        private CommandBuffer           _commandBuffer;
        private readonly RenderDevice   _renderDevice;
        private readonly Vk             _vk;

        private int                     _currentShaderId;
        private bool                    _bInRenderPass = false;

        static readonly uint            MaxTextureSlots = 4;

        private RenderTexture2D[]       _textures = new RenderTexture2D[MaxTextureSlots];


    }
}
