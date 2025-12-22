namespace UOEngine.Runtime.RHI;

public interface IRhiVertexBuffer
{
    uint Size { get; }
    uint Stride { get; }

    public void SetData(int offsetInBytes, IntPtr data, int dataLength);

    public void Upload();

    public void CleanUp();
}
