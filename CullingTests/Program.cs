using System;
using System.Collections.Generic;
using Engine;
using Engine.Initialization;
using System.Numerics;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering;
using Engine.Rendering.Shared.Culling;

namespace CullingTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Init.InitEngine(0,0, 1600, 900, "FrustrumTest", new GameTestCulling());
        }
    }

    class GameTestCulling : Game
    {
        public override void Gamestart()
        {
            base.Gamestart();
            new Player();
            new FrustrumTest(Camera.MainCamera);

        }
    }

    class FrustrumTest : Entity
    {
        public struct Plane
        {
            public Vector3 Normal;
            public float Offset;

            public Plane(Vector3 p0, Vector3 p1, Vector3 p2)
            {
                Normal = Vector3.Normalize(Vector3.Cross(p1 - p0, p2 - p1));
                Offset = Vector3.Dot(Normal, p0);
            }
        }

        public FrustrumTest(Camera baseCamera)
        {
            Vector3 nearCenter = op_Subtraction(Vector3.Zero, baseCamera.Front) * baseCamera.NearPlane;
            Vector3 farCenter = op_Subtraction(Vector3.Zero ,baseCamera.Front) * baseCamera.FarPlane;
            
            float nearHeight = (float)(2 * Math.Tan(baseCamera.GetFov()/ 2) * baseCamera.NearPlane);
            float farHeight = (float)(2 * Math.Tan(baseCamera.GetFov() / 2) * baseCamera.FarPlane);
            float nearWidth = nearHeight * baseCamera.AspectRatio;
            float farWidth = farHeight * baseCamera.AspectRatio;
            
            Vector3 farTopLeft = farCenter + baseCamera.Up * (farHeight*0.5f) - baseCamera.Right * (farWidth*0.5f);
            Vector3 farTopRight = farCenter + baseCamera.Up * (farHeight*0.5f) + baseCamera.Right * (farWidth*0.5f);
            Vector3 farBottomLeft = farCenter - baseCamera.Up * (farHeight*0.5f) - baseCamera.Right * (farWidth*0.5f);
            Vector3 farBottomRight = farCenter - baseCamera.Up * (farHeight*0.5f) + baseCamera.Right * (farWidth*0.5f);


            
            Vector3 nearTopLeft = nearCenter +  baseCamera.Up * (nearHeight*0.5f) - baseCamera.Right * (nearWidth*0.5f);
            Vector3 nearTopRight = nearCenter +  baseCamera.Up * (nearHeight*0.5f) + baseCamera.Right * (nearWidth*0.5f);
            Vector3 nearBottomLeft = nearCenter -  baseCamera.Up * (nearHeight*0.5f) - baseCamera.Right * (nearWidth*0.5f);
            Vector3 nearBottomRight = nearCenter -  baseCamera.Up * (nearHeight*0.5f) + baseCamera.Right * (nearWidth*0.5f);


            MeshData front = new MeshData(new List<Vector3>() { nearTopRight, nearTopLeft, nearBottomLeft, nearBottomRight },
                new List<Vector2> { Vector2.Zero, new Vector2(0, 1), new Vector2(1, 0), new Vector2(1f, 1) });
            MeshData back = new MeshData(new List<Vector3>() { farTopLeft, farTopLeft, farBottomLeft, farBottomRight },
                new List<Vector2> { Vector2.Zero, new Vector2(0, 1), new Vector2(1, 0), new Vector2(1f, 1) });
            MeshData left = new MeshData(new List<Vector3>() { nearTopLeft, farTopLeft, farBottomLeft, nearBottomLeft },
                new List<Vector2> { Vector2.Zero, new Vector2(0, 1), new Vector2(1, 0), new Vector2(1f, 1) });
            front.QueueVaoRegen();
            back.QueueVaoRegen();
            left.QueueVaoRegen();
            
            //frustum.planes[FRUSTUM_PLANES::FRONT] = calculate_plane(ntr, ntl, nbl);
            //frustum.planes[FRUSTUM_PLANES::BACK] = calculate_plane(ftl, ftr, fbr);
            //frustum.planes[FRUSTUM_PLANES::LEFT] = calculate_plane(ntl, ftl, nbl);
            //frustum.planes[FRUSTUM_PLANES::RIGHT] = calculate_plane(ftr, ntr, fbr);
            //frustum.planes[FRUSTUM_PLANES::TOP] = calculate_plane(ntl, ntr, ftl);
            //frustum.planes[FRUSTUM_PLANES::BOTTOM] = calculate_plane(nbl, fbl, fbr);
        }

        static Vector3 op_Subtraction(Vector3 first, Vector3 second)
        {
            return Vector3.Subtract(first, second);
        }

        public override void _Ready()
        {
            base._Ready();
        }
    }
}