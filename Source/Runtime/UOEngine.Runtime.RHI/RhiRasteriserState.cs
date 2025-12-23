namespace UOEngine.Runtime.RHI;

public enum RhiCullMode: byte
{
    Disable,
    Back,
    Front
}

public enum RhiFillMode: byte
{
    Solid,
    Wireframe
}

public enum RhiFrontFace: byte
{
    Clockwise,
    CounterClockwise
}

public record struct RhiRasteriserState
{
    public RhiCullMode CullMode;
    public RhiFillMode FillMode;

    public RhiFrontFace FrontFace;

    public string? Name;

    public static readonly RhiRasteriserState CullCounterClockwise = new("CullCounterClockwise", RhiCullMode.Back);


    public RhiRasteriserState()
    {
        CullMode = RhiCullMode.Back;
        FillMode = RhiFillMode.Solid;
        FrontFace = RhiFrontFace.CounterClockwise;
    }

    private RhiRasteriserState(string name, RhiCullMode cullMode): this()
    {
        Name = name;
        CullMode = cullMode;
    }
}
