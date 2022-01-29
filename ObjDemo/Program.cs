#nullable enable
using System;
using System.Diagnostics;
using System.Numerics;
using Engine;
using Engine.Initialization;
using Engine.Input;
using Engine.Objects;
using Engine.Renderable;
using Engine.Rendering;
using Engine.Rendering.Culling;
using Silk.NET.Input;

namespace ObjDemo
{
    internal class ObjDemo : Game
    {
        public override void Gamestart()
        {
            base.Gamestart();

            FpsEntity entity = new FpsEntity();
            Camera fpCam = new Camera(new Vector3(0, 0, 0), -Vector3.UnitZ, Vector3.UnitX , 1.777778F, true );
            MeshSpawner thing = new MeshSpawner();
            
            InputHandler.SetMouseMode(0, CursorMode.Normal);

            //BenchmarkEntity entTest = new BenchmarkEntity();
        }
    }

    internal class Entrypoint
    {
        static void Main()
        {
            Init.InitEngine( 0,0, 1600, 900, "Hello World", new ObjDemo());
        }
    }

    internal class MeshSpawner : GameObject
    {
        Mesh[] _meshes;

        public override void _Ready()
        {
            base._Ready();
            //ImGUI_ModelViewer viewer = new ImGUI_ModelViewer();

            //new Player();
            
            //_meshes = AssimpLoader.LoadMesh(@"C:\Users\donov\Downloads\130\scene.gltf", this);

            foreach (var mesh in _meshes)
            {
                mesh.Scale = .1f;
                mesh.QueueVaoRegen();  
            }
        }
    }

    internal class FpsEntity : Entity
    {
        Stopwatch _fpstimer = Stopwatch.StartNew();
        int _frames;

       
        public override void _Process(double delta)
        {
            _frames+=1;
            if (_fpstimer.Elapsed.Seconds >= 1)
            {
                Console.WriteLine($"Elapsed: {(_fpstimer.Elapsed.Seconds * 1000)}");
                Console.WriteLine($"Frames: {_frames}");
                _frames = 0;
                _fpstimer.Restart();
            }
        }
    }


}