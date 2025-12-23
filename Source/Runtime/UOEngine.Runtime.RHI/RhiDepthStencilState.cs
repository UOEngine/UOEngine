using System.Xml.Linq;

namespace UOEngine.Runtime.RHI;

enum RhiStencilOperation
{
    Keep,
    Zero
}

public struct RhiDepthStencilState
{
    public bool DepthBufferEnable;
    public bool DepthBufferWriteEnable;

    public bool StencilEnable;

    public string? Name;

    public static readonly RhiDepthStencilState None = new(
        "DepthStencilState.None",
        false,
        false
    );

    public RhiDepthStencilState()
    {
        DepthBufferEnable = false;
        DepthBufferWriteEnable = false;
        StencilEnable = false;
    }

    private RhiDepthStencilState(string name, bool depthBufferEnable, bool depthBufferWriteEnable)
    {
        Name = name;
        DepthBufferEnable = depthBufferEnable;
        DepthBufferWriteEnable = depthBufferWriteEnable;
    }
}
