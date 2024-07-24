using UOEngine.Runtime.Rendering;

namespace UOEngine.Apps.Client
{
    internal class SimpleShader: Shader
    {
        public SimpleShader()
        {
            Name = "Simple";
        }

        protected override void SetupVertexShaderDescriptors(List<SetBindingDescription> descriptors)
        {
        }

        protected override void SetupFragmentShaderDescriptors(List<SetBindingDescription> descriptors)
        {
            var uoImage = new SetBindingDescription
            {
                DescriptorType = EDescriptorType.CombinedSampler,
                Binding = 0,
                Layout = 0
            };

            descriptors.Add(uoImage);
        }
    }
}
