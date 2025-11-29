using System.Diagnostics;

using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

[DebuggerDisplay("{Name}")]
public class EffectParameter
{
    internal readonly Effect Parent;
    internal readonly ShaderVariable Info;
    internal IntPtr values;

    public string Name { get; }

    internal EffectParameter(Effect parent, string name, in ShaderVariable info)
    {
        Parent = parent;
        Info = info;
        Name = name;
    }

    #region SetValue overloads

    public unsafe void SetValue(float value)
    {
        fixed (byte* p = &Parent.Data.ConstantBuffer[Info.Offset])
            *((float*)p) = value;
    }

    public unsafe void SetValue(Vector3 value)
    {
        fixed (byte* p = &Parent.Data.ConstantBuffer[Info.Offset])
            *((Vector3*)p) = value;
    }

    public unsafe void SetValue(Vector4 value)
    {
        // XNA uses Matrix as 4x4 floats in row-major
        fixed (byte* p = &Parent.Data.ConstantBuffer[Info.Offset])
            *((Vector4*)p) = value;
    }

    public unsafe void SetValue(Matrix value)
    {
        // XNA uses Matrix as 4x4 floats in row-major
        fixed (byte* p = &Parent.Data.ConstantBuffer[Info.Offset])
            *((Matrix*)p) = value;
    }

    public void SetValue(float[] values)
    {
        Buffer.BlockCopy(values, 0, Parent.Data.ConstantBuffer, (int)Info.Offset, (int)Info.Size);
    }

    #endregion
}