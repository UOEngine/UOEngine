namespace Microsoft.Xna.Framework.Graphics;

public class DepthStencilState
{
    public bool DepthBufferEnable;
    public bool DepthBufferWriteEnable;
    public bool StencilEnable;
    public CompareFunction DepthBufferFunction;

    public static readonly DepthStencilState None;
}
