using System.Xml.Linq;

namespace Microsoft.Xna.Framework.Graphics;

public class DepthStencilState
{
    public static readonly DepthStencilState None = new DepthStencilState(
    "DepthStencilState.None",
    false,
    false);

    private DepthStencilState(
    string name,
    bool depthBufferEnable,
    bool depthBufferWriteEnable
    )
    {
        Name = name;
        DepthBufferEnable = depthBufferEnable;
        DepthBufferWriteEnable = depthBufferWriteEnable;
    }
}
