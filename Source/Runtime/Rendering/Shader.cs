namespace UOEngine.Runtime.Rendering
{
    public struct SetBindingDescription
    {
        public EDescriptorType DescriptorType;
        public uint Binding;
        public uint Layout;
    }
    public class Shader
    {
        public Shader()
        {
            _vertexShaderPath = "";
            _fragmentShaderPath = "";
            Name = "";
        }

        virtual protected void SetupVertexShaderDescriptors(List<SetBindingDescription> descriptors)
        {

        }
        virtual protected void SetupFragmentShaderDescriptors(List<SetBindingDescription> descriptors)
        {

        }

        public void Setup()
        {
            SetupVertexShaderDescriptors(_vertexShaderDescriptors);
            SetupFragmentShaderDescriptors(_fragmentShaderDescriptors);
        }

        public string Name { get; protected set; }

        public IReadOnlyList<SetBindingDescription> GetVertexShaderDescriptors() => _vertexShaderDescriptors;
        public IReadOnlyList<SetBindingDescription> GetFragmentShaderDescriptors() => _fragmentShaderDescriptors;

        private List<SetBindingDescription> _vertexShaderDescriptors = [];
        private List<SetBindingDescription> _fragmentShaderDescriptors = [];

        protected string _vertexShaderPath;
        protected string _fragmentShaderPath;
    }
}
