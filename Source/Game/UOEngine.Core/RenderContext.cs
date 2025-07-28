using UOEngine.Core;
using UOEngine.Interop;

namespace UOEngine
{
    public static class RenderContext
    {
        public static void SetShaderBindingData(Texture2D texture)
        {
            RenderContextNative.SetShaderBindingData(texture.NativeHandle);
        }

        public static ShaderInstance GetShaderInstance()
        {
            UIntPtr shaderHandle = RenderContextNative.GetShaderInstance();

            return new ShaderInstance(shaderHandle);
        }

        public static void Draw()
        {
            RenderContextNative.Draw();
        }
    }
}
