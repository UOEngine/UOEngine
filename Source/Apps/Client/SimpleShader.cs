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
        }
    }
}
