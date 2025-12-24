using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexPositionColorTexture : IVertexType
{
    VertexDeclaration IVertexType.VertexDeclaration
    {
        get
        {
            return VertexDeclaration;
        }
    }

    public Vector3 Position;
    public Color Color;
    public Vector2 TextureCoordinate;

    public static readonly VertexDeclaration VertexDeclaration;

    static VertexPositionColorTexture()
    {
        VertexDeclaration = new VertexDeclaration(
            [
                new VertexElement(
                    0,
                    VertexElementFormat.Vector3,
                    VertexElementUsage.Position,
                    0
                ),
                new VertexElement(
                    12,
                    VertexElementFormat.Color,
                    VertexElementUsage.Color,
                    0
                ),
                new VertexElement(
                    16,
                    VertexElementFormat.Vector2,
                    VertexElementUsage.TextureCoordinate,
                    0
                )
            ]
        );
    }

}
