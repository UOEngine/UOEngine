using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.Runtime.RHI.Resources;

public interface IRenderIndexBuffer
{
    public ushort[] Indices { get;}

    public void SetData(ushort[] data)
    {
        Buffer.BlockCopy(data, 0, Indices, 0, data.Length * sizeof(ushort));
    }

    public void Upload();
}
