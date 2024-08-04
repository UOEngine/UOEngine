using UOEngine.Runtime.Rendering;

namespace UOEngine.Apps.Client
{
    internal class SimpleShader: Shader
    {
        public SimpleShader()
        {
            Name = "Simple";
            _vertexShaderName = "vert.spv";
            _fragmentShaderName = "frag.spv";

            var uoImage = new SetBindingDescription
            {
                DescriptorType = EDescriptorType.CombinedSampler,
                ShaderType = EShaderType.Fragment,
                Binding = 0,
                Layout = 0
            };

            _descriptors.Add(uoImage);
        }
    }
}
