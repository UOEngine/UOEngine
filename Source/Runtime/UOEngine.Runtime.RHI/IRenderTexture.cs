using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.Runtime.RHI;

public interface IRenderTexture
{
    public void SetDataPointer(UIntPtr pointer, int size);
}
