// Copyright (c) 2025 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Numerics;
using System.Runtime.InteropServices;

namespace UOEngine.Runtime.RHI;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct ModelViewProjection
{
    public Matrix4x4 View;
    public Matrix4x4 Projection;
}

public interface IRenderContext
{
    public ShaderInstance ShaderInstance { get; set; }

    public IRhiIndexBuffer IndexBuffer { set; }

    public IRhiVertexBuffer VertexBuffer { get; set; }

    public RhiSampler Sampler { get; set; }


    public void BeginRenderPass(in RenderPassInfo renderPassInfo);
    public void EndRenderPass();

    //public void BeginRecording();
    public void EndRecording();

    public void DrawIndexedPrimitives(uint numIndices, uint numInstances, uint firstIndex, uint vertexOffset, uint firstInstance);


    public void SetGraphicsPipeline(in RhiGraphicsPipelineDescription graphicsPipelineDescription);

    public void WaitForGpuIdle();

}
