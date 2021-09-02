using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Numerics;
using Sledge.DataStructures.Geometric.Precision;
using Plane = Sledge.DataStructures.Geometric.Precision.Plane;

namespace MapConverter
{
    static class MapFileLib
    {
        static Side ConvertSide(Node sideNode)
        {
            string texture = sideNode.GetNodebyName("Texture").Keyvalue;
            string xOffset = sideNode.GetNodebyName("X_offset").Keyvalue;
            string yOffset = sideNode.GetNodebyName("Y_offset").Keyvalue;
            string xScale = sideNode.GetNodebyName("X_scale").Keyvalue;
            string yScale = sideNode.GetNodebyName("Y_scale").Keyvalue;
            string rotAngle = sideNode.GetNodebyName("Rot_angle").Keyvalue;
            string id = sideNode.GetNodebyName("ID").Keyvalue;

            Node[] pointnodes = sideNode.GetNodesWithStringInName("PointLocation", false);
            Vector3[] faceCoords = new Vector3[3];
            int index = 0;
            foreach (Node pointnode in pointnodes)
            {
                faceCoords[index] = MathLib.ThreeNumberstringsToVector3(pointnode.Keyvalue, ' ');
                index++;
            }

            Plane thing = new Plane();
            return new Side(texture, faceCoords, Convert.ToSingle(xOffset), Convert.ToSingle(yOffset), Convert.ToSingle(rotAngle), Convert.ToSingle(yScale), Convert.ToSingle(xScale), Convert.ToInt32(id));
        }

        static Brush ConvertBrush(Node brushNode)
        {
            Brush currentBrush = new Brush
            {
                Id = Convert.ToInt32(brushNode.Keyvalue)
            };
            List<Side> sides = new List<Side>();
            foreach (Node sideNode in brushNode.GetNodesWithStringInName("Side", false))
            {

                sides.Add(ConvertSide(sideNode));

            }
            currentBrush.Sides = sides.ToArray();
            return currentBrush;
        }

        static BrushEntity ConvertBrushEntity(Node entity, Dictionary<string, string> attributesDictionary)
        {
            Node[] brushes = entity.GetNodesByName("Brush");
            List<Brush> brushList = new List<Brush>();
            foreach (Node brushNode in brushes)
            {
                brushList.Add(ConvertBrush(brushNode));
            }
            string classname = attributesDictionary["classname"];
            attributesDictionary.Remove("classname");
            return new BrushEntity(attributesDictionary, classname, brushList.ToArray());
        }

        public static Map ConvertMap(Node parseTree)
        {
            Map worldStruct = new Map();
            List<PointEntity> pEntities = new List<PointEntity>();
            List<BrushEntity> bEntities = new List<BrushEntity>();


            foreach (Node entity in parseTree.Children)
            {
                bool isBrushEntity = CheckIsBrushEntity(entity);

                // Get Attributes!
                Dictionary<string, string> attributesDictionary = GetAttributes(entity);
                if (isBrushEntity)
                {
                    bEntities.Add(ConvertBrushEntity(entity, attributesDictionary));
                }
                else
                {
                    string origin = attributesDictionary["origin"];
                    string classname = attributesDictionary["classname"];
                    attributesDictionary.Remove("classname");
                    attributesDictionary.Remove("origin");
                    pEntities.Add(new PointEntity(MathLib.ThreeNumberstringsToVector3(origin, " "), attributesDictionary, classname));

                }
            }
            worldStruct.PEntities = pEntities.ToArray();
            worldStruct.BEntities = bEntities.ToArray();
            return worldStruct;
        }

        static Dictionary<string, string> GetAttributes(Node entity)
        {
            Node[] attributes = entity.GetNodesWithStringInName("Attribute:", true);
            Dictionary<string, string> attributesDictionary = new Dictionary<string, string>();
            foreach (Node attribute in attributes)
            {
                attributesDictionary.Add(attribute.Name, attribute.Keyvalue);
            }
            return attributesDictionary;
        }

        static bool CheckIsBrushEntity(Node entity)
        {
            Node brushes = entity.GetNodebyName("Brush");
            if (brushes != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Map ImportMapFile(string path)
        {
            Console.WriteLine("Loading File");
            StreamReader fs = new StreamReader(path);
            string fileString = fs.ReadToEnd();
            Console.WriteLine("Creating Parse tree");
            Node worldandEntities = Preprocessing.Break_Entities(fileString);
            for (int i = 0; i < worldandEntities.Children.Count; i++)
            {
                Node entity = worldandEntities.Children[i];
                NewParser.ParseEntity(ref entity);
            }
            Map worldstruct = ConvertMap(worldandEntities);

            return worldstruct;
        }
    }
}

