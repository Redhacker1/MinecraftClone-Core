using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using Engine;
using Engine.Objects;
using Engine.Rendering;
using MapConverter;
using Silk.NET.Maths;
using Sledge.DataStructures.Geometric.Precision;
using Plane = System.Numerics.Plane;

namespace LevelLoaderTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Engine.Initialization.Init.InitEngine(0,0, 1600, 900, "LevelLoader", new MapLoader());
        }

        class MapLoader : Game
        {
            public override void Gamestart()
            {
                base.Gamestart();
                new Camera(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY, 16 / 9, true);

                Map mapdata = MapConverter.MapFileLib.ImportMapFile(@"C:\Users\donov\Documents\GitHub\Quake_source_Tools\maps\arcane_dimentions_data\maps\ad_sepulcher.map");
                Console.WriteLine("Assembling points");
                

                foreach (var BrushEntity in mapdata.BEntities)
                {
                    foreach (var brush in BrushEntity.Brushes)
                    {
                        var C = new List<Vector3>();
                        foreach (var side in brush.Sides)
                        {
                            Plane plane = Plane.CreateFromVertices(side.Points[0], side.Points[1], side.Points[2]);
                            var v = new List<Vector3>();
                            foreach (var Point in side.Points)
                            {
                                float Distance = PointDistanceOnplane(Point, plane.Normal, plane.D);
                                // Behind
                                if (Distance < 0.001f)
                                {
                                    v.Add(Point);
                                }
                            }

                        }
                        
                        
                        /*
                        // Split the polygon by all the other planes
                         
                        var poly = new Polygon(list[i]);
                        for (var j = 0; j < list.Count; j++)
                        {
                            if (i != j && poly.Split(list[j], out var back, out _))
                            {
                                poly = back;
                            }
                        }
                        polygons.Add(poly);
                        */
                    }
                }

                foreach (var PointEntities in mapdata.PEntities)
                {
                    Entity OtherEntity = new Entity(PointEntities.Location,Vector2.Zero);
                }

            }

            float PointDistanceOnplane(Vector3 Point, Vector3 Normal, float Distance)
            {
                return ((Point.X * Normal.X) + (Point.Y * Normal.Y) + (Point.Z * Normal.Z)) - Distance;
            }
            
        }
    }
}