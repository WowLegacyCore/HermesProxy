using System.Numerics;

namespace HermesProxy.Framework.Math
{
    public struct Position
    {
        public Position(float x = 0.0f, float y = 0.0f, float z = 0.0f, float orientation = 0.0f)
        {
            X = x;
            Y = y;
            Z = z;
            Orientation = orientation;
        }

        public Position(Vector3 vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
            Orientation = 0.0f;
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Orientation { get; set; }

        public Vector3 ToVector3() => new(X, Y, Z);
    }
}
