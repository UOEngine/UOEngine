using System.Runtime.InteropServices;

using UOEngine.Runtime.Core;

namespace UOEngine.Runtime.Rendering
{
    public struct VertexBindingDescription
    {
        public uint Binding;
        public uint Stride;
    }

    public struct VertexAttributeDescription
    {
        public uint             Binding;
        public uint             Location;
        public EVertexFormat    VertexFormat;
        public int              Offset;
    }

    public enum EVertexFormat
    {
        None,
        R32G32SignedFloat,
        R32G32B32A32SignedFloat,
        R8G8B8A8UNorm = 37

    }

    public class Shader
    {
        public Shader()
        {
            VertexShaderName = "";
            FragmentShaderName = "";
            Name = "";
        }

        public void Setup()
        {
            _vertexByteCode = GetShaderByteCode(VertexShaderName);
            _fragmentByteCode = GetShaderByteCode(FragmentShaderName);
        }

        protected uint GetOffset<T>(string name)
        {
            return (uint)Marshal.OffsetOf<T>(name).ToInt32();
        }

        private byte[] GetShaderByteCode(string file)
        {
            var shaderDirectory = Path.Combine(Paths.Intermediate, "Shaders");

            return File.ReadAllBytes(Path.Combine(shaderDirectory, file));
        }

        public string                                       Name { get; protected set; }

        public string                                       VertexShaderName { get; protected set; }
        public string                                       FragmentShaderName { get; protected set; }

        public IReadOnlyList<VertexBindingDescription>      VertexBindingDescriptions => _vertexBindingDescriptions;

        public IReadOnlyList<VertexAttributeDescription>    VertexAttributeDescriptions => _vertexAttributeDescriptions;

        public ReadOnlySpan<byte>                           VertexByteCode => _vertexByteCode;
        public ReadOnlySpan<byte>                           FragmentByteCode => _fragmentByteCode;

        protected List<VertexAttributeDescription>          _vertexAttributeDescriptions = [];
        protected List<VertexBindingDescription>            _vertexBindingDescriptions = [];

        private byte[]                                      _vertexByteCode = [];
        private byte[]                                      _fragmentByteCode = [];
    }
}
