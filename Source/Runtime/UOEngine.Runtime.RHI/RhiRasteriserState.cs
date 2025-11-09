namespace UOEngine.Runtime.RHI;

public enum RhiCullMode: byte
{
    Disable,
    Back,
    Front
}

public enum RhiFillMode: byte
{
    Fill,
    Wireframe
}

public struct RhiRasteriserState
{
    public RhiCullMode CullMode { get; set; } = RhiCullMode.Disable;
    public RhiFillMode FillMode { get; set; } = RhiFillMode.Fill;

    public RhiRasteriserState()
    {

    }
}
