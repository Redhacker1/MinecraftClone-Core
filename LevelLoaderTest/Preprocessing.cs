using System;
using System.Collections.Generic;
using System.Text;

namespace MapConverter
{
    class Preprocessing
    {
        public static string[] BreakBrushes(Node entity)
        {
            StringBuilder currentBrush = new StringBuilder();
            int brushCount = 0;
            List<string> brushes = new List<string>();
            char previousValue = '\n';
            foreach (char character in entity.Keyvalue)
            {
                if (previousValue == '{' && character == '\n')
                {
                    currentBrush.Clear();
                    brushCount++;
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

        public static Node Break_Entities(string mapFile)
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
                                Node entityNode = new Node("Entity")
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


    }
}
