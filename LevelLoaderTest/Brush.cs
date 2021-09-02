using System;
using System.Collections.Generic;
using System.Numerics;

namespace MapConverter
{
    struct Brush
    {
        public Side[] Sides { get; set; }
        public int Id { get; set; }
        
        public Brush(Side[] sides, int id)
        {
            Sides = sides;
            Id = id;
        }
    }
    struct Side
    {
        public double DistanceFromOrigin { get; }
        public Side(string texture, Vector3[] points, float xOffset, float yOffset, float rotAngle, float yScale, float xScale, int id)
        {
            var ab = points[1] - points[0];
            var ac = points[2] - points[0];

            Normal = Vector3.Cross(ac,ab);
            DistanceFromOrigin = Vector3.Dot(Normal,points[0]);
            PointOnPlane = points[0];

            this.Normal.X = Normal.X;
            this.Normal.Y = Normal.Y;
            this.Normal.Z = Normal.Z;
            D = -DistanceFromOrigin;
            
            Texture = texture;
            Points = points;
            XOffset = xOffset;
            YOffset = yOffset;
            RotAngle = rotAngle;
            YScale = yScale;
            XScale = xScale;
            Id = id;
            UAxis = new Vector3(0, 0, 0);
            VAxis = new Vector3(0, 0, 0);
            Normal = MathLib.CalculateNormal(points[0], points[1], points[2]);
        }

        public string Texture { get; set; }
        public Vector3[] Points { get; set; }
        
        public double D { get; }
        public Vector3 PointOnPlane { get; }
        public float XOffset { get; set; }
        public float YOffset { get; set; }
        public float RotAngle { get; set; }
        public float YScale { get; set; }
        public float XScale { get; set; }
        public Vector3 UAxis { get; set; }
        public Vector3 VAxis { get; set; }
        public Vector3 Normal;
        public int Id { get; set; }

    }
    class PointEntity
    {
        public PointEntity(Vector3 location, Dictionary<string, string> attributes, string classname, bool isbrushentity = false)
        {
            Location = location;
            Attributes = attributes;
            Classname = classname;
            IsBrushEntity = isbrushentity;
        }

        public bool IsBrushEntity { get; set; }
        public Vector3 Location { get; set; }
        public Dictionary<string,string> Attributes { get; set; }
        public string Classname { get; set; }
    }

    class BrushEntity : PointEntity
    {
        public BrushEntity(Dictionary<string, string> attributes, string classname, Brush[] brushes) : base(new Vector3(0.0f, 0.0f, 0.0f ), attributes, classname, true)
        {
            Brushes = brushes;
        }
        public Brush[] Brushes { get; set; }
    }

    struct Map
    {
        public Map(PointEntity[] pentities, BrushEntity[] bentities)
        {
            PEntities = pentities;
            BEntities = bentities;
        }

        public PointEntity[] PEntities { get; set; }
        public BrushEntity[] BEntities { get; set; }

    }
}
