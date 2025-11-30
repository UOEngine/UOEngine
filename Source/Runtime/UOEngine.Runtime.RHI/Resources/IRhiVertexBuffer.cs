namespace UOEngine.Runtime.RHI;

public interface IRhiVertexBuffer
{
    public void SetData(int offsetInBytes, IntPtr data, int dataLength);
}
