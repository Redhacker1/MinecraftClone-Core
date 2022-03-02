using System;
using System.Numerics;

namespace Engine.MathLib
{
    public static class Vector3Extension
    {

        public static Vector3 Floor (this Vector3 vector3)
        {
            return new Vector3((float) Math.Floor(vector3.X),(float) Math.Floor(vector3.Y), (float) Math.Floor(vector3.Z));
        }
    }
}