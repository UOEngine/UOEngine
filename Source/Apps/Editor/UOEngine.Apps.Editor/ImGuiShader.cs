using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using UOEngine.Runtime.Rendering;

namespace UOEngine.Apps.Editor
{
    internal class ImGuiShader : Shader
    {
        struct Vertex
        {
            public Vector2 Position = Vector2.Zero;
            public Vector2 TexCoord = Vector2.Zero;
            public Vector4 Colour = Vector4.Zero;

            public Vertex()
            {

            }
        }
        public ImGuiShader()
        {
            Name = "ImGuiShader";
            VertexShaderName = "ImGui.vert.spv";
            FragmentShaderName = "ImGui.frag.spv";

            //var projectionMatrix = new SetBindingDescription
            //{
            //    DescriptorType = EDescriptorType.UniformBuffer,
            //    ShaderStage = EShaderStage.Vertex,
            //    Binding = 0,
            //    Layout = 0
            //};

            //_descriptors.Add(projectionMatrix);

            //var fontSampler = new SetBindingDescription
            //{
            //    DescriptorType = EDescriptorType.CombinedSampler,
            //    ShaderStage = EShaderStage.Fragment,
            //    Binding = 1,
            //    Layout = 0
            //};

            //_descriptors.Add(fontSampler);

            _vertexBindingDescriptions.Add(new()
            {
                Binding = 0,
                Stride = GetOffset<Vertex>("Position"),
            });

            _vertexAttributeDescriptions.Add(new()
            {
                Binding = 0,
                Location = 0,
                VertexFormat = EVertexFormat.R32G32SignedFloat,
                Offset = Marshal.OffsetOf<Vertex>("Position").ToInt32(),
            });

            _vertexAttributeDescriptions.Add(new()
            {
                Binding = 0,
                Location = 1,
                VertexFormat = EVertexFormat.R32G32SignedFloat,
                Offset = Marshal.OffsetOf<Vertex>("TexCoord").ToInt32(),
            });

            _vertexAttributeDescriptions.Add(new()
            {
                Binding = 0,
                Location = 2,
                VertexFormat = EVertexFormat.R32G32B32A32SignedFloat,
                Offset = Marshal.OffsetOf<Vertex>("Colour").ToInt32()
            });
        }

    }
}
