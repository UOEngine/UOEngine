using Silk.NET.Vulkan;
using System.Diagnostics;

namespace UOEngine.Runtime.Rendering
{
    public class RenderDeviceContext
    {
        //public RenderDeviceContext()
        //{
        //    _vk = Vk.GetApi();
        //}

        //public void Tick()
        //{
        //    RenderDevice!.OnFrameBegin();

        //    ImmediateCommandList = RenderDevice.CurrentCommandBuffer;

        //    RenderDevice.BeginRenderPass();

        //    if (_bindIndexBuffer)
        //    {
        //        _vk.CmdBindIndexBuffer(RenderDevice.CurrentCommandBuffer, _indexBuffer!.DeviceBuffer, 0, IndexType.Uint16);
        //    }

        //    BindShader();

        //    _vk.CmdDrawIndexed(RenderDevice.CurrentCommandBuffer, _indexBuffer!.Length, 1, 0, 0, 0);

        //    RenderDevice.EndRenderPass();

        //    RenderDevice.Submit();
        //}

        //public void SetIndexBuffer(RenderBuffer indexBuffer)
        //{
        //    if(indexBuffer != null)
        //    {
        //        _bindIndexBuffer = true;
        //        _indexBuffer = indexBuffer;

        //        return;
        //    }

        //    _bindIndexBuffer = false;
        //    _indexBuffer = null;
        //}

        //public void SetTexture(int slot, RenderTexture2D texture)
        //{ 
        //    _renderTextures[slot] = texture;
        //}

        //public void SetShader(int shaderId)
        //{
        //    if(shaderId == _currentShaderId)
        //    {
        //        return;
        //    }

        //    _currentShaderId = shaderId;
        //    _shaderDirty = true;

        //}

        //private unsafe void BindShader()
        //{
        //    PipelineStateObjectDescription pso = RenderDevice!.GetPipelineStateObjectDescription(_currentShaderId);

        //    _vk.CmdBindPipeline(RenderDevice.CurrentCommandBuffer, PipelineBindPoint.Graphics, pso.PSO);

        //    if(_shaderDirty)
        //    {
        //        RenderDevice.AllocateDescriptorSets(pso.DescriptorSetLayouts, out _descriptorSets);

        //        const int maxDescriptorImageInfos = 4;
        //        int numDescriptorImageInfos = 0;
        //        var descriptorImageInfos = stackalloc DescriptorImageInfo[maxDescriptorImageInfos];

        //        Span<WriteDescriptorSet> descriptorWrites = stackalloc WriteDescriptorSet[_descriptorSets.Length];

        //        int numUpdated = 0;

        //        foreach (var bindingDescription in pso.BindingDescriptions)
        //        {
        //            WriteDescriptorSet writeDescriptorSet = new WriteDescriptorSet();

        //            writeDescriptorSet.DstBinding = bindingDescription.Binding;
        //            writeDescriptorSet.SType = StructureType.WriteDescriptorSet;
        //            writeDescriptorSet.DescriptorCount = 1;
        //            writeDescriptorSet.DstSet = _descriptorSets[numUpdated];

        //            switch (bindingDescription.DescriptorType)
        //            {
        //                case EDescriptorType.CombinedSampler:
        //                    {
        //                        DescriptorImageInfo imageInfo = descriptorImageInfos[numDescriptorImageInfos];

        //                        imageInfo.ImageLayout = ImageLayout.ShaderReadOnlyOptimal;

        //                        imageInfo.ImageView = _renderTextures[bindingDescription.Binding]._imageView;
        //                        imageInfo.Sampler = RenderDevice.TextureSampler;

        //                        descriptorImageInfos[numDescriptorImageInfos] = imageInfo;

        //                        writeDescriptorSet.DescriptorType = DescriptorType.CombinedImageSampler;
        //                        writeDescriptorSet.PImageInfo = &descriptorImageInfos[numDescriptorImageInfos];

        //                        numDescriptorImageInfos++;
        //                    }
        //                    break;

        //                default:
        //                    {
        //                        Debug.Assert(false);
        //                    }
        //                    break;
        //            }

        //            descriptorWrites[numUpdated++] = writeDescriptorSet;
        //        }

        //        _vk.UpdateDescriptorSets(RenderDevice.Device, (uint)numUpdated, descriptorWrites, 0, []);

        //        _shaderDirty = false;
        //    }


        //    _vk.CmdBindDescriptorSets(RenderDevice.CurrentCommandBuffer, PipelineBindPoint.Graphics, pso.Layout, 0, _descriptorSets, null);

        //}

        //public RenderDevice?                 RenderDevice;

        //public RenderCommandListImmediate?   ImmediateCommandList { get; private set; }   
                                             
        //public int                           ShaderHash { get; set; }
                                             
        //private int                          _currentShaderId;
        //private bool                         _shaderDirty = true;     
                                             
        //public RenderBuffer?                 IndexBuffer { get; set; }
                                             
        //private readonly Vk                  _vk;
        //private RenderBuffer?                _indexBuffer;
        //private bool                         _bindIndexBuffer = false;
        //private readonly RenderTexture2D[]   _renderTextures = new RenderTexture2D[2];

        //private DescriptorSet[]?             _descriptorSets;
    }
}
