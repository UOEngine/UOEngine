using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ImGuiNET;

using UOEngine.Runtime.Rendering;

namespace UOEngine.Apps.Editor
{
    internal class ImGuiShader : Shader
    {
        public ImGuiShader()
        {
            Name = "ImGuiShader";
            VertexShaderName = "ImGui.vert.spv";
            FragmentShaderName = "ImGui.frag.spv";

            _vertexBindingDescriptions.Add(new()
            {
                Binding = 0,
                Stride = (uint)Unsafe.SizeOf<ImDrawVert>(),
            });

            _vertexAttributeDescriptions.Add(new()
            {
                Binding = 0,
                Location = 0,
                VertexFormat = EVertexFormat.R32G32SignedFloat,
                Offset = Marshal.OffsetOf<ImDrawVert>("pos").ToInt32(),
            });

            _vertexAttributeDescriptions.Add(new()
            {
                Binding = 0,
                Location = 1,
                VertexFormat = EVertexFormat.R32G32SignedFloat,
                Offset = Marshal.OffsetOf<ImDrawVert>("uv").ToInt32(),
            });

            _vertexAttributeDescriptions.Add(new()
            {
                Binding = 0,
                Location = 2,
                VertexFormat = EVertexFormat.R8G8B8A8UNorm,
                Offset = Marshal.OffsetOf<ImDrawVert>("col").ToInt32()
            });

            BlendingDesc = new BlendingDescription(true);
        }

    }
}
