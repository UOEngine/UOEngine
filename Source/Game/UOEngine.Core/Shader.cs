using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.Core;

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