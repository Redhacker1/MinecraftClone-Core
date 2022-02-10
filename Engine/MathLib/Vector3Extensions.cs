using System;
using Engine.MathLib.DoublePrecision_Numerics;

namespace Engine.MathLib
{
    public static class Vector3Extension
    {


        public static System.Numerics.Vector3 CastToNumerics (this Vector3 vector3)
        {
            return new System.Numerics.Vector3((float) vector3.X, (float) vector3.Y, (float) vector3.Z);
        }
        
        public static Vector3 Floor (this Vector3 vector3)
        {
            return new Vector3((float) Math.Floor(vector3.X),(float) Math.Floor(vector3.Y), (float) Math.Floor(vector3.Z));
        }
        
        
        public static Vector3 CastToDouble (this System.Numerics.Vector3 vector3)
        {
            return new Vector3(vector3.X, vector3.Y, vector3.Z);
        }

    }
}