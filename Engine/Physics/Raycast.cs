using System;
using Engine.MathLib;
using Vector3 = Engine.MathLib.DoublePrecision_Numerics.Vector3;

namespace Engine.Physics
{
    public static class Raycast
    {

        public static HitResult CastToPoint(Vector3 startDir, Vector3 endLocation, float debugtime)
        {
            Vector3 delta = endLocation - startDir;
            Vector3 deltaNormal = Vector3.Normalize(delta);

            int lineLength = (int) Math.Floor(delta.Length());

            return CastInDirection(startDir, deltaNormal, debugtime, lineLength);
        }
        
        public static HitResult CastInDirection(Vector3 origin, Vector3 direction, float debugtime, int maxVoxeldistance = 100)
        {
            HitResult outResult = new HitResult();
            Vector3 position = origin + new Vector3(.5f, .5f, .5f);


            Vector3 sign = new Vector3
            {
                X = direction.X > 0 ? 1 : 0,
                Y = direction.Y > 0 ? 1 : 0,
                Z = direction.Z > 0 ? 1 : 0
            }; // dda stuff



            for (int i = 0; i < maxVoxeldistance; ++i)
            {
                Vector3 tvec = ((position + sign).Floor() - position) / direction;
                
                double t = Math.Min(tvec.X, Math.Min(tvec.Y, tvec.Z));

                position += direction * (t + 0.001f); // +0.001 is an epsilon value so that you dont get precision issues
                //position.Floor();


                byte id = 0;

                // TODO: Add Collision masks to allow for more selective picking of blocks in the world.
                if (id != 0) // 0 here, just means air. This statement just says that you've hit a block IF the current ray march step *isnt* air.
                {
                    Vector3 normal = new Vector3();
                    
                    
                    normal.X = (t == tvec.X) ? 1 : 0;
                    if (sign.X == 1)
                    {
                        normal.X = -normal.X;
                    }
                    
                    normal.Y = (t == tvec.Y) ? 1 : 0;
                    if (sign.Y == 1)
                    {
                        normal.Y = -normal.Y;
                    }
                    
                    normal.Z = (t == tvec.Z) ? 1 : 0;
                    if (sign.Z == 1)
                    {
                        normal.Z = -normal.Z;
                    }

                    outResult.Normal = normal;
                    outResult.Hit = true;
                    
                    break;
                }

                outResult.Location = position;


            }


            return outResult;
        }
    }
}