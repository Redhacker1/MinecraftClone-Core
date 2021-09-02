
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MapConverter
{
    static class NewParser
    {

        static public Node Break_Entities(string mapFile)
        {
            Node world = new Node("World");
            int entityReset = 0;
            int entityCount = 0;

            string[] fileLines = mapFile.Split('\n');

            for (int increment = 0; increment < fileLines.Length; increment++)
            {
                fileLines[increment] += '\n';
            }
            StringBuilder currentEntity = new StringBuilder();
            char previousChar = '\0';

            foreach (string line in fileLines)
            {
                foreach (char character in line)
                {
                    if (character == (char)13)
                    {
                        if (previousChar == '}')
                        {
                            --entityReset;
                            if (entityReset == 0)
                            {
                                if (currentEntity[currentEntity.Length - 1] == '}')
                                {
                                    currentEntity.Remove(currentEntity.Length - 1, 1);
                                }
                                Node entityNode = new Node(string.Format("Entity", entityCount))
                                {
                                    Keyvalue = currentEntity.ToString()
                                };
                                world.Children.Add(entityNode);
                                entityCount++;
                            }
                        }
                        else if (previousChar == '{')
                        {
                            ++entityReset;
                            if (entityReset == 1)
                            {
                                currentEntity.Clear();
                            }
                        }
                    }
                    else
                    {
                        currentEntity.Append(character);
                        previousChar = character;
                    }
                }

            }

            return world;
        }

        static void PullKeyValuePairs(ref Node entity)
        {
            StringBuilder @string = new StringBuilder();
            bool instring = false;
            bool key = true;
            Node keyValuePair = new Node("Blank");
            for (int i = 0; i < entity.Keyvalue.Length; i++)
            {

                char character = entity.Keyvalue[i];
                if (character == '"')
                {
                    if (instring)
                    {
                        if (key)
                        {
                            keyValuePair = new Node("Attribute:" + @string.ToString().Trim());
                            key = false;
                        }
                        else
                        {
                            keyValuePair.Keyvalue = @string.ToString().Trim();
                            entity.Children.Add(keyValuePair);
                            key = true;
                        }
                        instring = false;
                    }
                    else
                    {
                        @string.Clear();
                        instring = true;
                    }
                }
                else
                {
                    @string.Append(character);
                }
            }
        }
        public static Node ImportMapFile(string path)
        {
            Console.WriteLine("Loading File");
            StreamReader fs = new StreamReader(path);
            string fileString = fs.ReadToEnd();
            Console.WriteLine("Creating Parse tree");
            Node worldandEntities = Break_Entities(fileString);
            for (int i = 0; i < worldandEntities.Children.Count; i++)
            {
                Node entity = worldandEntities.Children[i];
                ParseEntity(ref entity);
            }
            ConvertMap(worldandEntities);

            return worldandEntities;
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
            if(brushes != null)
            {
                return true;
            }
            else
            {
                return false;
            }
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
                    Node[] brushes = entity.GetNodesByName("Brush");
                    List<Brush> brushList = new List<Brush>();
                    foreach (Node brushNode in brushes)
                    {
                        Brush currentBrush = new Brush
                        {
                            Id = Convert.ToInt32(brushNode.Keyvalue)
                        };
                        List<Side> sides = new List<Side>();
                        foreach (Node sideNode in brushNode.GetNodesWithStringInName("Side", false))
                        {
                            //Console.WriteLine(Side.Childcount(true));
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

                            sides.Add(new Side(texture, faceCoords, Convert.ToSingle(xOffset), Convert.ToSingle(yOffset), Convert.ToSingle(rotAngle), Convert.ToSingle(yScale), Convert.ToSingle(xScale), Convert.ToInt32(id)));

                        }
                        currentBrush.Sides = sides.ToArray();
                        brushList.Add(currentBrush);
                    }
                    string classname = attributesDictionary["classname"];
                    attributesDictionary.Remove("classname");
                    bEntities.Add(new BrushEntity(attributesDictionary, classname, brushList.ToArray()));
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

        static void ParseFace(ref Node brush, string brushString)
        {
            string[] sides = brushString.Trim().Split('\n');
            int sideId = 0;
            foreach (string side in sides)
            {
                Node sidenode = new Node("Side");
                string[] sideComponents = side.Trim().Split(" ");
                StringBuilder a = new StringBuilder();
                string[] tupleList = new string[3];
                int index = 0;

                foreach (char character in side)
                {
                    if (character == '(')
                    {
                        a.Clear();
                    }

                    else if (character == ')')
                    {
                        Node coords = new Node($"PointLocation_{index}");
                        coords.Keyvalue = a.ToString();
                        sidenode.Children.Add(coords);
                        index++;
                    }
                    else
                    {
                        a.Append(character);
                    }
                }

                //Texture
                bool success = float.TryParse(sideComponents[15], out _);

                string texture;
                bool offsetByOne = false;
                if (success)
                {
                    Console.WriteLine("Failed!, Trying value before!");
                    success = float.TryParse(sideComponents[14], out _);
                    offsetByOne = true;
                    if (!success)
                    {
                        Console.WriteLine("Success, Value needs to be offset by 1");
                        texture = sideComponents[14];
                        Console.WriteLine(texture);
                    }
                    else
                    {
                        throw new FormatException(message: "The brush side is incorrect!");
                    }
                }
                else
                {
                    texture = sideComponents[15];
                }
                Node textureNode = new Node("Texture")
                {
                    Keyvalue = texture
                };
                sidenode.Children.Add(textureNode);

                //X offset 
                string xOff = offsetByOne ? sideComponents[15] : sideComponents[16];
                Node xOffsetNode = new Node("X_offset")
                {
                    Keyvalue = xOff
                };
                sidenode.Children.Add(xOffsetNode);

                // Y Offset
                string yOff = offsetByOne ? sideComponents[16] : sideComponents[17];
                Node yOffsetNode = new Node("Y_offset")
                {
                    Keyvalue = yOff
                };
                sidenode.Children.Add(yOffsetNode);

                //Rotation
                string rotation = offsetByOne ? sideComponents[17] : sideComponents[18];
                Node rotationNode = new Node("Rot_angle")
                {
                    Keyvalue = rotation
                };
                sidenode.Children.Add(rotationNode);

                //X Scale 
                string xScale = !offsetByOne ? sideComponents[19] : sideComponents[18];

                Node xScaleNode = new Node("X_scale")
                {
                    Keyvalue = xScale
                };
                sidenode.Children.Add(xScaleNode);

                //Y Scale 
                string yScale = offsetByOne ? sideComponents[19] : sideComponents[20];
                Node yScaleNode = new Node("Y_scale")
                {
                    Keyvalue = yScale
                };
                sidenode.Children.Add(yScaleNode);
                Node idNode = new Node("ID")
                {
                    Keyvalue = $"{sideId}"
                };
                sidenode.Children.Add(yScaleNode);
                sidenode.Children.Add(idNode);
                sideId++;

                // Add Side to Brush
                brush.Children.Add(sidenode);
            }
        }
        static Node ParseBrush(string brush, int id)
        {
            Node brushNode = new Node("Brush");
            brushNode.Keyvalue = id.ToString();
            ParseFace(ref brushNode, brush);
            return brushNode;
        }

        static void ParseBrushes(ref Node entity)
        {
            IEnumerable<string> brushes = BreakBrushes(entity);
            int brushId = 0;
            foreach (string brush in brushes)
            {
                Node brushNode = ParseBrush(brush, brushId);
                brushId++;
                entity.Children.Add(brushNode);
            }
        }

        public static void ParseEntity(ref Node entity)
        {
            // Actually all we need to parse a Point entity!
            PullKeyValuePairs(ref entity);
            //Checks to see if we have a brush entity... because they require a LOT more work and a LOT more memory
            if (entity.Keyvalue.Contains('{') & entity.Keyvalue.Contains('}'))
            {
                ParseBrushes(ref entity);
            }
        }
    

        static IEnumerable<string> BreakBrushes(Node entity)
        {
            StringBuilder currentBrush = new StringBuilder();
            List<string> brushes = new List<string>();
            char previousValue = '\n';
            foreach (char character in entity.Keyvalue)
            {
                if (previousValue == '{' && character == '\n')
                {
                    currentBrush.Clear();
                }
                else if (character == '}' && previousValue == '\n')
                {
                    brushes.Add(currentBrush.ToString());
                }
                else
                {
                    currentBrush.Append(character);
                }
                previousValue = character;
            }
            entity.Keyvalue = string.Empty;
            return brushes.ToArray();
        }
    }
}
