namespace UOEngine.Runtime.Rendering
{

    public class Shader
    {
        public Shader()
        {
            _vertexShaderName = "";
            _fragmentShaderName = "";
            Name = "";
        }

        public void Setup()
        {
            _vertexByteCode = File.ReadAllBytes($"shaders/{_vertexShaderName}");
            _fragmentByteCode = File.ReadAllBytes($"shaders/{_fragmentShaderName}");
        }

        public string Name { get; protected set; }

        public IReadOnlyList<SetBindingDescription> GetDescriptors() => _descriptors;

        public ReadOnlySpan<byte> VertexByteCode => _vertexByteCode;
        public ReadOnlySpan<byte> FragmentByteCode => _fragmentByteCode;

        protected List<SetBindingDescription>   _descriptors = [];

        protected string                        _vertexShaderName;
        protected string                        _fragmentShaderName;

        private byte[]                          _vertexByteCode = [];
        private byte[]                          _fragmentByteCode = [];
    }
}
