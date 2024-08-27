using System.Diagnostics;

using Silk.NET.Vulkan;
using UOEngine.Runtime.Rendering.Resources;
using Buffer = Silk.NET.Vulkan.Buffer;
using VkSemaphore = Silk.NET.Vulkan.Semaphore;


namespace UOEngine.Runtime.Rendering
{
    public class RenderCommandListContext: IDisposable
    {
        public RenderCommandBufferManager   CommandBufferManager { get; private set; }
        public RenderCommandBuffer          ActiveCommandBuffer => CommandBufferManager.ActiveCommandBuffer!;

        private RenderPass                  _activeRenderPass;
        private RenderFramebuffer?          _activeFramebuffer;

        private Shader?                     _activeShader;

        public RenderCommandListContext(RenderDevice renderDevice)
        {
            _renderDevice = renderDevice;

            CommandBufferManager = new RenderCommandBufferManager(renderDevice);

            CommandBufferManager.Initialise();

            _vk = Vk.GetApi();
        }

        public void Dispose()
        {
            SubmitAndWaitUntilGPUIdle();
        }

        public void BeginRenderPass(in RenderPassInfo renderPassInfo, in RenderTargetInfo renderTargetInfo)
        {
            RenderPass renderPass = _renderDevice.GetOrCreateRenderPass(renderPassInfo);

            RenderFramebuffer framebuffer = _renderDevice.GetOrCreateFrameBuffer(renderTargetInfo, renderPass);
            
            CommandBufferManager.ActiveCommandBuffer!.BeginRenderPass(renderPass, framebuffer);

            _activeRenderPass = renderPass;
            _activeFramebuffer = framebuffer;
        }

        public void EndRenderPass()
        {
            CommandBufferManager.ActiveCommandBuffer!.EndRenderPass();
        }

        public void SetRenderTarget()
        {

        }

        public void CopyBuffer(Buffer srcBuffer, Buffer destBuffer, BufferCopy copyRegion)
        {
            _vk.CmdCopyBuffer(CommandBufferManager.ActiveCommandBuffer!.Handle, srcBuffer, destBuffer, 1, ref copyRegion);

        }
        //public void CopyBufferToImage(Buffer buffer, Image image, ImageLayout imageLayout, uint regionCount, BufferImageCopy bufferImageCopy)
        //{
        //    _vk.CmdCopyBufferToImage(CommandBufferManager.ActiveCommandBuffer!.Handle, buffer, image, ImageLayout.TransferDstOptimal, regionCount, ref bufferImageCopy);
        //}

        public void BindIndexBuffer(RenderBuffer buffer)
        {
            _vk.CmdBindIndexBuffer(CommandBufferManager.ActiveCommandBuffer!.Handle, buffer.DeviceBuffer, 0, IndexType.Uint16);
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

        //public void PipelineBarrier(PipelineStageFlags sourceStage, PipelineStageFlags destinationStage, ImageMemoryBarrier imageMemoryBarrier)
        //{
        //    unsafe
        //    {
        //        _vk.CmdPipelineBarrier(CommandBufferManager.ActiveCommandBuffer!.Handle, sourceStage, destinationStage, 0, 0, null, 0, null, 1, ref imageMemoryBarrier);
        //    }
        //}

        public void DrawIndexed(uint indexCount, uint instanceCount)
        {
            _vk.CmdDrawIndexed(CommandBufferManager.ActiveCommandBuffer!.Handle, indexCount, instanceCount, 0, 0, 0);
        }

        public unsafe void BindShader(Shader shader)
        {
            _activeShader = shader;

            PipelineStateObjectDescription pso = _renderDevice.GetOrCreatePipelineStateObject(_activeShader, _activeRenderPass);

            _vk.CmdBindPipeline(CommandBufferManager.ActiveCommandBuffer!.Handle, PipelineBindPoint.Graphics, pso.GraphicsPipeline);

            _renderDevice.AllocateDescriptorSets(pso.DescriptorSetLayouts, out DescriptorSet[] descriptorSets);

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
                    case DescriptorType.CombinedImageSampler:
                        {
                            DescriptorImageInfo imageInfo = descriptorImageInfos[numDescriptorImageInfos];

                            imageInfo.ImageLayout = ImageLayout.ShaderReadOnlyOptimal;

                            imageInfo.ImageView = _textures[bindingDescription.Binding].ShaderResourceView!.Value;
                            imageInfo.Sampler = _renderDevice.TextureSampler;

                            descriptorImageInfos[numDescriptorImageInfos] = imageInfo;

                            writeDescriptorSet.DescriptorType = DescriptorType.CombinedImageSampler;
                            writeDescriptorSet.PImageInfo = &descriptorImageInfos[numDescriptorImageInfos];
                            writeDescriptorSet.DstSet = descriptorSets[numUpdated];

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

            Vulkan.VkCmdBindDescriptorSets(CommandBufferManager.ActiveCommandBuffer!.Handle, PipelineBindPoint.Graphics, pso.Layout, descriptorSets);

        }

        public void SubmitAndWaitUntilGPUIdle()
        {
            Submit();

            _renderDevice.WaitUntilIdle();
        }

        public void Submit()
        {
            CommandBufferManager.SubmitActive();
        }

        private readonly RenderDevice       _renderDevice;
        private readonly Vk                 _vk;


        static readonly uint                MaxTextureSlots = 4;

        private RenderTexture2D[]           _textures = new RenderTexture2D[MaxTextureSlots];



    }
}
