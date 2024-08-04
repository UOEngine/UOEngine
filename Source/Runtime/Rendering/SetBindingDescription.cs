
namespace UOEngine.Runtime.Rendering
{
    public enum EShaderType
    {
        None,
        Vertex,
        Fragment
    }
    public struct SetBindingDescription
    {
        public EDescriptorType DescriptorType = EDescriptorType.None;
        public uint Binding = uint.MaxValue;
        public uint Layout = uint.MaxValue;
        public EShaderType ShaderType = EShaderType.None;

        public SetBindingDescription()
        {

        }
    }
}
