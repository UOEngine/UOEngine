using Microsoft.Xna.Framework;

namespace UOEngine.Runtime;

public class CameraEntity: IEntity
{
    private bool _dirty = true;
    private Matrix _transform;
    private Vector2 _viewport;

    public Matrix Projection { get; private set; }
    public Matrix View { get; private set; }

    public CameraEntity()
    {
        View = Matrix.Identity;// CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);
    }

    public void Update(TimeSpan time)
    {
    }

    public void SetProjection(float width, float height, float zNearPlane, float zFarPlane)
    {
        Projection = Matrix.CreateOrthographic(width, height, zNearPlane, zFarPlane);
    }
}
