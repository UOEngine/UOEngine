using System.Numerics;
using System.Runtime.InteropServices;
using UOEngine.Runtime.RHI.Resources;

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

    public IGraphicsPipeline GraphicsPipline { get; set; }

    public IRhiIndexBuffer IndexBuffer { get; set; }

    public IRhiVertexBuffer VertexBuffer { get; set; }

    public ModelViewProjection MVP { get; set; }

    public void BeginRenderPass(in RenderPassInfo renderPassInfo);
    public void EndRenderPass();

    public void BeginRecording();
    public void EndRecording();

    public void DrawIndexedPrimitives(uint numInstances);

}
