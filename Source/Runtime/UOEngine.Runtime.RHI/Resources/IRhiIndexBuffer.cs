namespace UOEngine.Runtime.RHI;

public interface IRhiIndexBuffer
{
    public void SetData(int offsetInBytes, IntPtr data, int byteLength);

    public void SetData(ReadOnlySpan<ushort> data);

    public void Upload();
}
