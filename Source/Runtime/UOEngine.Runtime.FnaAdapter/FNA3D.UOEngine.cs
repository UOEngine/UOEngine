using UOEngine.Runtime.FnaAdapter;
using UOEngine.Runtime.Renderer;
using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

internal static partial class FNA3D
{
    public struct FNDA3DUOEngine
    {
        public IRenderResourceFactory RenderResourceFactory;
        public IRenderDevice RenderDevice;
        public RenderSystem RenderSystem;
        public Remapper ShaderRemapper;
    }

    private static FNDA3DUOEngine _Fna3DUOEngine;

    public static void UOEngineSetup(in FNDA3DUOEngine Fna3DUOEngine)
    {
        _Fna3DUOEngine = Fna3DUOEngine;
    }

    public static void FNA3D_CreateEffect(byte[] effectCode, out UOEEffect effect)
    {
        _Fna3DUOEngine.ShaderRemapper.GetEffect(effectCode, out effect);

    }
}
