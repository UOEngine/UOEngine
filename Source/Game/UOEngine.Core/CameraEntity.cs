using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.Core;

public class CameraEntity : IEntity
{
    public Transform Transform { get; set; }

    public Matrix4x4 Projection { get; set; }

    public Matrix4x4 WorldToCameraMatrix => GetWorldToCameraMatrix();

    public CameraEntity()
    {
        Transform = new Transform();
    }

    private Matrix4x4 GetWorldToCameraMatrix()
    {
        Vector3 cameraToWorld = Transform.Position;

        Vector3 worldToCamera = -cameraToWorld;

        return Matrix4x4.Translate(worldToCamera);
    }
}
