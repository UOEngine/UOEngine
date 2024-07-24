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
            _vertexShaderName = "";
            _fragmentShaderName = "";
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
            _vertexByteCode = File.ReadAllBytes($"shaders/{_vertexShaderName}");
            _fragmentByteCode = File.ReadAllBytes($"shaders/{_fragmentShaderName}");

            SetupVertexShaderDescriptors(_vertexShaderDescriptors);
            SetupFragmentShaderDescriptors(_fragmentShaderDescriptors);
        }

        public string Name { get; protected set; }

        public IReadOnlyList<SetBindingDescription> GetVertexShaderDescriptors() => _vertexShaderDescriptors;
        public IReadOnlyList<SetBindingDescription> GetFragmentShaderDescriptors() => _fragmentShaderDescriptors;

        public ReadOnlySpan<byte> VertexByteCode => _vertexByteCode;
        public ReadOnlySpan<byte> FragmentByteCode => _fragmentByteCode;

        private List<SetBindingDescription> _vertexShaderDescriptors = [];
        private List<SetBindingDescription> _fragmentShaderDescriptors = [];

        protected string                    _vertexShaderName;
        protected string                    _fragmentShaderName;

        private byte[]                      _vertexByteCode = [];
        private byte[]                      _fragmentByteCode = [];
    }
}
