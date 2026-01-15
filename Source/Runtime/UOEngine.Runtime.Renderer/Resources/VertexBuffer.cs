// Copyright (c) 2025 - 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UOEngine.Runtime.Core;
using UOEngine.Runtime.RHI;

namespace UOEngine.Runtime.Renderer;

public class VertexBuffer<TVertex> where TVertex : unmanaged, IVertex, IVertexLayoutProvider
{
    public readonly IRhiBuffer RhiBuffer;

    public readonly TVertex[] Data;

    public RhiVertexDefinition VertexDefinition = TVertex.Layout;

    public uint Stride => TVertex.Layout.Stride;

    public int Capacity => Data.Length;
    public int Count { get; private set; }
    

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

    public void SetData()
    { 
        // Upload it.
        RhiBuffer.SetData(Data.AsSpan(0, Count));
    }

    public void Add(TVertex vertex)
    {
        UOEDebug.Assert(Count < Data.Length, "VertexBuffer overflow");

        Data[Count++] = vertex;
    }

    public void AddRange(ReadOnlySpan<TVertex> vertices)
    {
        UOEDebug.Assert(Count + vertices.Length > Data.Length, "VertexBuffer overflow");

        vertices.CopyTo(Data.AsSpan(Count));

        Count += vertices.Length;
    }

    public Span<TVertex> GetWriteSpan(int count)
    {
        UOEDebug.Assert(count >= 0);
        UOEDebug.Assert(Count + count < Data.Length);

        return Data.AsSpan(Count, count);
    }
}

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