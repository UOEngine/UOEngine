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

    internal ReadOnlySpan<byte> Data => _data;

    internal Texture texture;

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

    internal EffectParameter(Effect parent, string name, in ShaderParameter info)
    {
        // Should be a texture.
        Debug.Assert(info.InputType == RhiShaderInputType.Texture);

        Info.Type = RhiShaderVariableType.Invalid;

        Parent = parent;
        Name = name;
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
            fixed (byte* p = _data)
                *((Vector3*)p) = value;
        }
    }

    public unsafe void SetValue(Vector4 value)
    {
        unsafe
        {
            fixed (byte* p = _data)
                *((Vector4*)p) = value;

            //float* dstPtr = (float*)_data;
            //dstPtr[0] = value.X;
            //dstPtr[1] = value.Y;
            //dstPtr[2] = value.Z;
            //dstPtr[3] = value.W;
        }
    }

    public unsafe void SetValue(Matrix value)
    {
        // XNA uses Matrix as 4x4 floats in row-major
        fixed (byte* p = _data)
            *((Matrix*)p) = value;
    }

    public void SetValue(float[] values)
    {
        Buffer.BlockCopy(values, 0, Parent.Data.ConstantBuffer, (int)Info.Offset, (int)Info.Size);
    }

    public void SetValue(Texture value)
    {
        texture = value;
    }

    #endregion
    public Texture2D GetValueTexture2D()
    {
        return (Texture2D)texture;
    }
}