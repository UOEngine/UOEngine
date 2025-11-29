using System.Diagnostics;
using System.Runtime.InteropServices;
using UOEngine.Runtime.RHI;

namespace Microsoft.Xna.Framework.Graphics;

[DebuggerDisplay("{Name}")]
public class EffectParameter
{
    internal readonly Effect Parent;
    internal readonly ShaderVariable Info;
    internal IntPtr values { get; private set; }

    public string Name { get; }

    private readonly byte[] _data;
    private readonly GCHandle _pinned;
    private readonly int _offset;   // offset into the parent constant buffer

    internal EffectParameter(Effect parent, string name, in ShaderVariable info)
    {
        Parent = parent;
        Info = info;
        Name = name;

        _data = new byte[info.Size];
        _pinned = GCHandle.Alloc(_data, GCHandleType.Pinned);

        values = _pinned.AddrOfPinnedObject();
    }

    #region SetValue overloads

    public unsafe void SetValue(float value)
    {
        unsafe
        {
            float* dstPtr = (float*)values;
            *dstPtr = value;
        }
    }

    public unsafe void SetValue(Vector3 value)
    {
        unsafe
        {
            float* dstPtr = (float*)values;
            dstPtr[0] = value.X;
            dstPtr[1] = value.Y;
            dstPtr[2] = value.Z;
        }
    }

    public unsafe void SetValue(Vector4 value)
    {
        unsafe
        {
            float* dstPtr = (float*)values;
            dstPtr[0] = value.X;
            dstPtr[1] = value.Y;
            dstPtr[2] = value.Z;
            dstPtr[3] = value.W;
        }
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