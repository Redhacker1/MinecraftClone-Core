#nullable enable
using System;
using System.Diagnostics;
using System.Numerics;
using Engine;
using Engine.AssetLoading;
using Engine.Initialization;
using Engine.Input;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering;
using Silk.NET.Input;

namespace ObjDemo
{
    class ObjDemo : Game
    {
        public override void Gamestart()
        {
            base.Gamestart();

            FPSEntity entity = new FPSEntity();
            Camera fpCam = new Camera(new Vector3(0, 0, 0), -Vector3.UnitZ, Vector3.UnitY , 1.777778F, true );
            MeshSpawner thing = new MeshSpawner();
            
            InputHandler.SetMouseMode(0, CursorMode.Normal);

            //BenchmarkEntity entTest = new BenchmarkEntity();
        }
    }

    class Entrypoint
    {
        static void Main()
        {
            Init.InitEngine( 0,0, 1600, 900, "Hello World", new ObjDemo());
        }
    }

    class MeshSpawner : GameObject
    {
        Mesh[] meshes;

        protected override void _Ready()
        {
            base._Ready();
            ImGUI_ModelViewer viewer = new ImGUI_ModelViewer();

            new Player();
            
            meshes = AssimpLoader.LoadMesh(@"C:\Users\donov\Downloads\130\scene.gltf", this);

            if (meshes != null)
            {
                foreach (var mesh in meshes)
                {
                    mesh.Scale = .1f;
                    mesh.QueueVaoRegen();
                }   
            }
        }
    }
    class FPSEntity : Entity
    {
        Stopwatch fpstimer = Stopwatch.StartNew();
        int frames;
        public override void _Process(double delta)
        {
            frames+=1;
            if (fpstimer.Elapsed.Seconds >= 1)
            {
                Console.WriteLine($"Elapsed: {(fpstimer.Elapsed.Seconds * 1000)}");
                Console.WriteLine($"Frames: {frames}");
                frames = 0;
                fpstimer.Restart();
            }
        }
    }


}