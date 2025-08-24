using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.Core;

public class Transform
{
    public Vector3 Position { get; set; }

    public Quaternion Rotation { get; set; }

    public Vector3 Forward { get; set; }

    public Vector3 Up { get; set; }

    public Matrix4x4 LocalToWorldMatrix => GetMatrix();

    public Transform()
    {
        Forward = Vector3.Forward;
        Up = Vector3.Up;
        Rotation = Quaternion.Identity;

    }

    private Matrix4x4 GetMatrix()
    {
        Matrix4x4 matrix = Matrix4x4.Identity;

        return matrix;
    }
}
