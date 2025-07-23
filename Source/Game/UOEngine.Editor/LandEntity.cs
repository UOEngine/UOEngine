using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine;

internal class LandEntity: IEntity
{
    public readonly ushort GraphicId;

    public LandEntity(ushort graphicId)
    {
        GraphicId = graphicId;
    }
}
