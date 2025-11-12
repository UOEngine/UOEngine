namespace Microsoft.Xna.Framework.Graphics;

public class RasterizerState
{
    public CullMode CullMode;

    public static RasterizerState CullNone;

    public float DepthBias;
    public FillMode FillMode;
    public bool MultiSampleAntiAlias;

    public bool ScissorTestEnable;

    public float SlopeScaleDepthBias;
}
