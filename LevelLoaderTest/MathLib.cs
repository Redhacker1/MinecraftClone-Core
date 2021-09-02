using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace MapConverter
{
    static class MathLib
    {
        static public Vector3 CalculateNormal(Vector3 vector1, Vector3 vector2, Vector3 vector3)
        {
            return Vector3.Cross(vector2 - vector1, vector3 - vector1);
        }

        static public Vector3 CalulateNormal_UnitVector(Vector3 vector1, Vector3 vector2, Vector3 vector3)
        {
            return Vector3.Normalize(CalculateNormal(vector1, vector2, vector3));
        }

        static public double GetScalarValue(Vector3 vector, Vector3 normalVector)
        {
            return (normalVector.X * vector.X) + (normalVector.Y * vector.Y) + (normalVector.Z * vector.Z) - ShortestSignedDistance(normalVector, vector);
        }

        static public double ShortestSignedDistance(Vector3 normal, Vector3 point0)
        {
            return -(point0.X * normal.X + point0.Z * normal.Y + point0.Z * normal.Z);
        }

        static public double DistanceFromOrigin(Vector3 normal, Vector3 point)
        {
            return Vector3.Dot(normal, point);
        }
        static public Vector3 ThreeNumberstringsToVector3(string x, string y, string z)
        {
            return new Vector3(Convert.ToInt32(x.Trim()), Convert.ToInt32(y.Trim()), Convert.ToInt32(z.Trim()));
        }
        static public Vector3 ThreeNumberstringsToVector3(string spacedXyz, string splitter)
        {
            string[] xyzArray = spacedXyz.Split(splitter);
            return new Vector3(Convert.ToInt32(xyzArray[0].Trim()), Convert.ToInt32(xyzArray[1].Trim()), Convert.ToInt32(xyzArray[2].Trim()));
        }
        static public Vector3 ThreeNumberstringsToVector3(string spacedXyz, char splitter)
        {
            string[] xyzArray = spacedXyz.Trim().Split(splitter);
            return new Vector3(Convert.ToInt32(xyzArray[0].Trim()), Convert.ToInt32(xyzArray[1].Trim()), Convert.ToInt32(xyzArray[2].Trim()));
        }

    }
}
