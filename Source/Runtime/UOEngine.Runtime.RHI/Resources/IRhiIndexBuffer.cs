namespace UOEngine.Runtime.RHI;

public interface IRhiIndexBuffer
{
    public ushort[] Indices { get;}

    public void SetData(ushort[] data)
    {
        Buffer.BlockCopy(data, 0, Indices, 0, data.Length * sizeof(ushort));
    }

    public void SetData(ReadOnlySpan<ushort> data)
    {
        data.CopyTo(Indices);
    }

    public void Upload();
}
