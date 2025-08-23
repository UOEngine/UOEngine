using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.Core;

public class CameraEntity: IEntity
{
    public Matrix4x4 Transform = Matrix4x4.Identity;

    public Matrix4x4 Projection = Matrix4x4.Identity;

    public Matrix4x4 View => Matrix4x4.Identity;

    public CameraEntity()
    {

    }
}
