using System;

namespace Framework.GameMath;

public struct EulerAngles
{
    // All values as radians
    public double Roll;     // x
    public double Pitch;    // y
    public double Yaw;      // z

    public EulerAngles(double roll, double pitch, double yaw)
    {
        Roll = roll;
        Pitch = pitch;
        Yaw = yaw;
    }
    
    public Quaternion AsQuaternion()
    {
        double cy = Math.Cos(Yaw * 0.5);
        double sy = Math.Sin(Yaw * 0.5);
        double cp = Math.Cos(Pitch * 0.5);
        double sp = Math.Sin(Pitch * 0.5);
        double cr = Math.Cos(Roll * 0.5);
        double sr = Math.Sin(Roll * 0.5);

        Quaternion q = new Quaternion();
        q.W = (float)(cr * cp * cy + sr * sp * sy);
        q.X = (float)(sr * cp * cy - cr * sp * sy);
        q.Y = (float)(cr * sp * cy + sr * cp * sy);
        q.Z = (float)(cr * cp * sy - sr * sp * cy);
        return q;
    }
}
