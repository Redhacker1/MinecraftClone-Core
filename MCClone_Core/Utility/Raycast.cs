﻿using System;
using System.Numerics;
using Engine.MathLib;
using MCClone_Core.Physics;

namespace MCClone_Core.Utility
{
    public static class Raycast
    {

        public static HitResult CastToPoint(Vector3 StartDir, Vector3 EndLocation, float debugtime)
        {
            Vector3 delta = EndLocation - StartDir;
            Vector3 deltaNormal = Vector3.Normalize(delta);

            int lineLength = (int)Math.Floor(delta.Length());

            return CastInDirection(StartDir, deltaNormal, debugtime, lineLength);
        }

        public static HitResult CastInDirection(Vector3 Origin, Vector3 Direction, float debugtime, int MaxVoxeldistance = 100)
        {
            Vector3 NormDirection = Vector3.Normalize(Direction);

            // In which direction the voxel ids are incremented.
            int stepX = (int) getSignZeroPositive(Direction.X);
            int stepY = (int) getSignZeroPositive(Direction.Y);
            int stepZ = (int) getSignZeroPositive(Direction.Z);

            double tMaxX = Intbound(Origin.X + 0.5, NormDirection.X);
            double tMaxY = Intbound(Origin.Y + 0.5, NormDirection.Y);
            double tMaxZ = Intbound(Origin.Z + 0.5, NormDirection.Z);
            
            double tDeltaX = stepX / NormDirection.X;
            double tDeltaY = stepY / NormDirection.Y;
            double tDeltaZ = stepZ / NormDirection.Z;


            HitResult outResult = new HitResult();
            Vector3 position = Origin; //+ new Vector3(.5f, .5f, .5f);
            int BlockID = 0;
            
            for (int i = 0; i < MaxVoxeldistance; ++i)
            {
                if (tMaxX < tMaxY && tMaxX < tMaxZ) {
                    position.X += stepX;
                    tMaxX += tDeltaX;
                }
                else if (tMaxY < tMaxZ) {
                    position.Y += stepY;
                    tMaxY += tDeltaY;
                }
                else {
                    position.Z += stepZ;
                    tMaxZ += tDeltaZ;
                }
                
                //BlockID = ProcWorld.Instance.GetBlockIdFromWorldPos((int) position.X, (int) position.Y, (int) position.Z);
                if (BlockID != 0)
                {
                    outResult.Hit = true;
                    break;
                }
            }
            outResult.Location = position.Floor();
            return outResult;
        }

        public static long getNegativeSign(double number)
        {
            if (number >= 0)
            {
                return 0;
            }

            return -1;
        }
        
        public static long getSignZeroPositive(double number) 
        {
            return getNegativeSign(number) | 1;
        }

        static double Intbound(double s, double ds)
        {
            if (ds < 0 && Math.Abs(Math.Round(s) - s) < 0.001) return 0;
            s = Mod(s, 1);
            return (ds > 0 ? Math.Ceiling(s) - s : s - Math.Floor(s)) / Math.Abs(ds);
        }

        static double Mod(double value, double modulus)
        {
            return (value % modulus + modulus) % modulus;
        }
    }
}