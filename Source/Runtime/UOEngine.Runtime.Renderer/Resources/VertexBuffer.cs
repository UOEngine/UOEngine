// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Renderer;

public class VertexBuffer<TVertex> where TVertex : unmanaged, IVertex, IVertexLayoutProvider
{
    public readonly IRhiBuffer RhiBuffer;

    public readonly TVertex[] Data;

    public RhiVertexDefinition VertexDefinition = TVertex.Layout;

    public uint Stride => TVertex.Layout.Stride;

    internal VertexBuffer(IRenderResourceFactory factory, uint numVertices)
    {
        uint stride = (uint)Unsafe.SizeOf<TVertex>();

        RhiBuffer = factory.NewBuffer(new RhiBufferDescription
        {
            Size = numVertices * stride,
            Stride = stride,
            Usage = RhiBufferUsageFlags.Vertex | RhiBufferUsageFlags.Dynamic
        });

        Data = new TVertex[numVertices];
    }

    public void SetData(TVertex[] data)
    { 
        RhiBuffer.SetData<TVertex>(data.AsSpan());
    }
}

//public readonly record struct VertexElement(
//    string Semantic,
//    int Location,
//    RhiVertexAttributeFormat Format,
//    int OffsetBytes
//);

//public sealed class VertexLayout
//{
//    public uint StrideBytes { get; }
//    public VertexElement[] Elements { get; }

//    public VertexLayout(params VertexElement[] elements)
//    {
//        StrideBytes = 0;
//        Elements = elements;

//        foreach (var vertexElement in elements)
//        {
//            StrideBytes += SizeOfFormat(vertexElement.Format);
//        }
//    }
//    private static uint SizeOfFormat(RhiVertexAttributeFormat f) => f switch
//    {
//        RhiVertexAttributeFormat.Vector2 => 8,
//        RhiVertexAttributeFormat.Vector3 => 12,
//        RhiVertexAttributeFormat.Vector4 => 16,
//        RhiVertexAttributeFormat.R8G8B8A8_UNorm => 4,
//        _ => throw new NotSupportedException()
//    };
//}

public interface IVertexLayoutProvider
{
    static abstract RhiVertexDefinition Layout { get; }
}

public interface IVertex
{

}

[StructLayout(LayoutKind.Sequential)]
public struct PositionAndColourVertex: IVertex, IVertexLayoutProvider
{
    public Vector3 Position;
    public uint Colour;

    public static RhiVertexDefinition Layout => new(
        new RhiVertexAttribute(RhiVertexAttributeType.Position, RhiVertexAttributeFormat.Vector3, 0),
        new RhiVertexAttribute(RhiVertexAttributeType.Colour, RhiVertexAttributeFormat.R8G8B8A8_UNorm, 12));
}

[StructLayout(LayoutKind.Sequential)]
public struct PositionUVVertex : IVertex, IVertexLayoutProvider
{
    public Vector3 Position;
    public Vector2 UV;

    public static RhiVertexDefinition Layout => new(
        new RhiVertexAttribute(RhiVertexAttributeType.Position, RhiVertexAttributeFormat.Vector3, 0),
        new RhiVertexAttribute(RhiVertexAttributeType.TextureCoordinate, RhiVertexAttributeFormat.Vector2, 12));
}