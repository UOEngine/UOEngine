using UOEngine.Runtime.Rendering;

namespace UOEngine.Apps.Client
{
    internal class SimpleShader: Shader
    {
        public SimpleShader()
        {
            Name = "Simple";
            VertexShaderName = "simple.vert.spv";
            FragmentShaderName = "simple.frag.spv";

            var uoImage = new SetBindingDescription
            {
                DescriptorType = EDescriptorType.CombinedSampler,
                ShaderStage = EShaderStage.Fragment,
                Binding = 0,
                Layout = 0
            };

            _descriptors.Add(uoImage);
        }
    }
}
