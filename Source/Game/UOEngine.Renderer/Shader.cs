using UOEngine.Core;

namespace UOEngine.Renderer;

public class Shader<T>where T: struct
{
    public T ShaderData;
}

public struct DefaultShaderData
{
    public Matrix4x4 Projection;
    public RenderBuffer PerInstanceData;
}


public class DefaultShader: Shader<DefaultShaderData>
{
}