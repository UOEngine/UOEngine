
namespace UOEngine.Runtime.Rendering
{
    public enum EShaderStage
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
        public EShaderStage ShaderStage = EShaderStage.None;

        public SetBindingDescription()
        {

        }
    }
}
