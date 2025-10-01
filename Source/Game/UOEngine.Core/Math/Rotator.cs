using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOEngine.Core;

[DebuggerDisplay("Pitch = {Pitch} Yaw = {Yaw} Roll = {Roll}")]
public struct Rotator
{
    public float Pitch;
    public float Yaw;
    public float Roll;

    public Rotator(float pitch, float yaw, float roll)
    {
        Pitch = pitch;
        Yaw = yaw;
        Roll = roll;
    }

    public Rotator ToDegrees()
    {
        float conversion = 180 / MathF.PI;

        return conversion * new Rotator(Pitch, Yaw, Roll);
    }

    public static Rotator operator*(float scalar,  Rotator rotation)
    {
        return new Rotator(scalar * rotation.Pitch, scalar * rotation.Yaw, rotation.Roll);
    }

    public Quaternion ToQuaternion()
    {
        float cy = MathF.Cos(Yaw / 2);
        float sy = MathF.Sin(Yaw / 2);
        float cp = MathF.Cos(Pitch / 2);
        float sp = MathF.Sin(Pitch / 2);
        float cr = MathF.Cos(Roll / 2);
        float sr = MathF.Sin(Roll / 2);

        float w = cr * cp * cy + sr * sp * sy;
        float x = sr * cp * cy - cr * sp * sy;
        float y = cr * sp * cy + sr * cp * sy;
        float z = cr * cp * sy - sr * sp * cy;

        return new Quaternion(x, y, z, w);
    }
}
