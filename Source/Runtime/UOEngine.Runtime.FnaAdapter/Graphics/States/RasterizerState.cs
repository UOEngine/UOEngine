namespace Microsoft.Xna.Framework.Graphics;

public class RasterizerState
{
    public CullMode CullMode
    {
        get
        {
            return state.cullMode;
        }
        set
        {
            state.cullMode = value;
        }
    }

    public float DepthBias
    {
        get
        {
            return state.depthBias;
        }
        set
        {
            state.depthBias = value;
        }
    }

    public FillMode FillMode
    {
        get
        {
            return state.fillMode;
        }
        set
        {
            state.fillMode = value;
        }
    }

    public bool MultiSampleAntiAlias
    {
        get
        {
            return state.multiSampleAntiAlias == 1;
        }
        set
        {
            state.multiSampleAntiAlias = (byte)(value ? 1 : 0);
        }
    }

    public bool ScissorTestEnable
    {
        get
        {
            return state.scissorTestEnable == 1;
        }
        set
        {
            state.scissorTestEnable = (byte)(value ? 1 : 0);
        }
    }

    public float SlopeScaleDepthBias
    {
        get
        {
            return state.slopeScaleDepthBias;
        }
        set
        {
            state.slopeScaleDepthBias = value;
        }
    }
}
