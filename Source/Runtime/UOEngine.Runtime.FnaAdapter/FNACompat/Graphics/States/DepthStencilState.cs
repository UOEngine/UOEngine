using System.Xml.Linq;

namespace Microsoft.Xna.Framework.Graphics;

public class DepthStencilState
{
    public string Name;

    public bool DepthBufferEnable;
    public bool DepthBufferWriteEnable;
    public CompareFunction DepthBufferFunction;
    public bool StencilEnable;
    public int StencilMask;
    public int StencilWriteMask;
    public bool TwoSidedStencilMode;
    public StencilOperation StencilFail;
    public StencilOperation StencilDepthBufferFail;
    public StencilOperation StencilPass;
    public CompareFunction StencilFunction;
    public StencilOperation CounterClockwiseStencilFail;
    public StencilOperation CounterClockwiseStencilDepthBufferFail;
    public StencilOperation CounterClockwiseStencilPass;
    public CompareFunction CounterClockwiseStencilFunction;
    public int ReferenceStencil;

    public static readonly DepthStencilState None = new(
            "DepthStencilState.None",
            false,
            false
        );

    public DepthStencilState()
    {
        DepthBufferEnable = true;
        DepthBufferWriteEnable = true;
        DepthBufferFunction = CompareFunction.LessEqual;
        StencilEnable = false;
        StencilFunction = CompareFunction.Always;
        StencilPass = StencilOperation.Keep;
        StencilFail = StencilOperation.Keep;
        StencilDepthBufferFail = StencilOperation.Keep;
        TwoSidedStencilMode = false;
        CounterClockwiseStencilFunction = CompareFunction.Always;
        CounterClockwiseStencilFail = StencilOperation.Keep;
        CounterClockwiseStencilPass = StencilOperation.Keep;
        CounterClockwiseStencilDepthBufferFail = StencilOperation.Keep;
        StencilMask = Int32.MaxValue;
        StencilWriteMask = Int32.MaxValue;
        ReferenceStencil = 0;
    }

    private DepthStencilState(string name, bool depthBufferEnable, bool depthBufferWriteEnable)
    {
        Name = name;
        DepthBufferEnable = depthBufferEnable;
        DepthBufferWriteEnable = depthBufferWriteEnable;
    }
}
