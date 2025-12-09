namespace Microsoft.Xna.Framework.Graphics;

public class RasterizerState
{
    public CullMode CullMode;
    public string Name;

    public static RasterizerState CullNone = new("RasterizerState.CullNone", CullMode.None);

    public static readonly RasterizerState CullCounterClockwise = new("RasterizerState.CullCounterClockwise", CullMode.CullCounterClockwiseFace);

    public float DepthBias;
    public FillMode FillMode;
    public bool MultiSampleAntiAlias;

    public bool ScissorTestEnable;

    public float SlopeScaleDepthBias;

    public RasterizerState()
    {
        CullMode = CullMode.CullCounterClockwiseFace;
        FillMode = FillMode.Solid;
        DepthBias = 0;
        MultiSampleAntiAlias = true;
        ScissorTestEnable = false;
        SlopeScaleDepthBias = 0;
    }

    private RasterizerState(string name, CullMode cullMode)
    {
        Name = name;
        CullMode = cullMode;
    }
}
