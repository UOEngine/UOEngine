using System.Diagnostics;

using Silk.NET.Vulkan;
using UOEngine.Runtime.Rendering.Resources;
using Buffer = Silk.NET.Vulkan.Buffer;
using VkSemaphore = Silk.NET.Vulkan.Semaphore;


namespace UOEngine.Runtime.Rendering
{
    public class RenderCommandListContext: IDisposable
    {
        public Action<RenderCommandListContextImmediate>? Rendering;
        public RenderCommandBufferManager   CommandBufferManager { get; private set; }
        public RenderCommandBuffer          ActiveCommandBuffer => CommandBufferManager.ActiveCommandBuffer!;

        private RenderPass                  _activeRenderPass;
        private RenderFramebuffer?          _activeFramebuffer;

        private Shader?                     _activeShader;

        private readonly RenderDevice       _renderDevice;
        private readonly Vk                 _vk;

        static readonly uint                MaxTextureSlots = 4;
        private RenderTexture2D[]           _textures = new RenderTexture2D[MaxTextureSlots];

        private RenderUniformBuffer[]       _uniformBuffers = new RenderUniformBuffer[MaxTextureSlots];

        private List<RenderResource>        _pendingDeletes = [];

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

        public void CopyBuffer(Buffer srcBuffer, Buffer destBuffer, BufferCopy copyRegion)
        {
            _vk.CmdCopyBuffer(CommandBufferManager.ActiveCommandBuffer!.Handle, srcBuffer, destBuffer, 1, ref copyRegion);

        }

        public void BindIndexBuffer(RenderBuffer buffer)
        {
            _vk.CmdBindIndexBuffer(CommandBufferManager.ActiveCommandBuffer!.Handle, buffer.DeviceBuffer, 0, IndexType.Uint16);
        }

        public void BindVertexBuffer(RenderBuffer buffer)
        {
            Vulkan.VkBindVertexBuffer(CommandBufferManager.ActiveCommandBuffer!.Handle, buffer.DeviceBuffer);
        }

        public void BindUniformBuffer(RenderUniformBuffer buffer, uint slot)
        {
            _uniformBuffers[slot] = buffer;
        }

        public void SetTexture(RenderTexture2D texture, uint slot)
        {
            _textures[slot] = texture;
        }

        public void DrawIndexed(uint indexCount, uint instanceCount, uint firstIndex, int vertexOffset, uint instanceOffset)
        {
            _vk.CmdDrawIndexed(CommandBufferManager.ActiveCommandBuffer!.Handle, indexCount, instanceCount, firstIndex, vertexOffset, instanceOffset);
        }

        public unsafe void BindShader(Shader shader)
        {
            _activeShader = shader;

            PipelineStateObjectDescription pso = _renderDevice.GetOrCreatePipelineStateObject(_activeShader, _activeRenderPass);

            _vk.CmdBindPipeline(CommandBufferManager.ActiveCommandBuffer!.Handle, PipelineBindPoint.Graphics, pso.GraphicsPipeline);

            _renderDevice.AllocateDescriptorSets(pso.DescriptorSetLayouts, out DescriptorSet[] descriptorSets);

            const int   maxDescriptorImageInfos = 4;

            int         numDescriptorImageInfos = 0;
            int         numDescriptorBufferInfos = 0;

            var         descriptorImageInfos = stackalloc DescriptorImageInfo[maxDescriptorImageInfos];
            var         descriptorBufferInfos = stackalloc DescriptorBufferInfo[maxDescriptorImageInfos];

            Span<WriteDescriptorSet> descriptorWrites = stackalloc WriteDescriptorSet[descriptorSets.Length];

            int numUpdated = 0;

            foreach (var bindingDescription in pso.BindingDescriptions)
            {
                ref WriteDescriptorSet writeDescriptorSet = ref descriptorWrites[numUpdated];
              
                writeDescriptorSet.DstBinding = bindingDescription.Binding;
                writeDescriptorSet.SType = StructureType.WriteDescriptorSet;
                writeDescriptorSet.DescriptorCount = 1;
                writeDescriptorSet.DstSet = descriptorSets[numUpdated];

                switch (bindingDescription.DescriptorType)
                {
                    case DescriptorType.CombinedImageSampler:
                        {
                            ref DescriptorImageInfo imageInfo = ref descriptorImageInfos[numDescriptorImageInfos];

                            imageInfo.ImageLayout = ImageLayout.ShaderReadOnlyOptimal;
                            imageInfo.ImageView = _textures[bindingDescription.Binding].ShaderResourceView!.Value;
                            imageInfo.Sampler = _renderDevice.TextureSampler;

                            writeDescriptorSet.DescriptorType = DescriptorType.CombinedImageSampler;
                            writeDescriptorSet.PImageInfo = &descriptorImageInfos[numDescriptorImageInfos];

                            numDescriptorImageInfos++;

                            break;
                        }

                    case DescriptorType.UniformBuffer:
                        {
                            ref DescriptorBufferInfo descriptorBufferInfo = ref descriptorBufferInfos[numDescriptorBufferInfos];

                            descriptorBufferInfo.Buffer = _uniformBuffers[bindingDescription.Binding].Handle;
                            descriptorBufferInfo.Offset = 0;
                            descriptorBufferInfo.Range = _uniformBuffers[bindingDescription.Binding].Size;

                            writeDescriptorSet.DescriptorType = DescriptorType.UniformBuffer;
                            writeDescriptorSet.PBufferInfo = &descriptorBufferInfos[numDescriptorBufferInfos];

                            numDescriptorBufferInfos++;

                            break;
                        }

                    default:
                        {
                            Debug.Assert(false);

                            break;
                        }
                }
                numUpdated++;
                //descriptorWrites[numUpdated++] = writeDescriptorSet;
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

        public void MarkResourceForDelete(RenderResource resource)
        {
            _pendingDeletes.Add(resource);
        }

        public void FlushPendingDeletes()
        {
            foreach(RenderResource resource in _pendingDeletes)
            {
                resource.Destroy();
            }

            _pendingDeletes.Clear();
        }
    }
}
