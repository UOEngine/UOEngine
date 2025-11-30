using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.Runtime.RHI;

public enum RhiBlendOperation
{

}

public enum RhiBlendFactor
{

}

public struct RhiBlendState
{
    public RhiBlendFactor SourceBlendFactor;
    public RhiBlendFactor DestinationBlendFactor;
    public RhiBlendOperation ColourBlendOp;
    public RhiBlendOperation AlphaBlendOperation;
}
