using System.Collections.Generic;

namespace MapConverter
{
    class Node
    {
        public Node(string name)
        {
            Name = name;
        }
        public string Name;
        public List<Node> Children = new List<Node>();
        public string Keyvalue;

        public int Childcount(bool recursive)
        {
            int selfChildCount = Children.Count;
            if (!recursive || Children.Count == 0)
            {
                return selfChildCount;
            }
            else
            {
                int regularcount = 0;
                foreach (Node thing in Children)
                {
                    regularcount += thing.Childcount(recursive);
                }
                return selfChildCount + regularcount;
            }

        }
        public Node GetNodebyName(string desiredName)
        {
            foreach(Node item in Children)
            {
                if(item.Name == desiredName)
                {
                    return item;
                }

            }
            return null;
        }
        public Node[] GetNodesByName(string desiredName)
        {
            List<Node> nodes = new List<Node>();
            foreach (Node item in Children)
            {
                if (item.Name == desiredName)
                {
                    nodes.Add(item);
                }
            }
            return nodes.ToArray();
        }
        public Node[] GetNodesWithStringInName(string desiredStringinName, bool removeStringInName)
        {
            List<Node> nodes = new List<Node>();
            foreach (Node item in Children)
            {
                if (item.Name.Contains(desiredStringinName))
                {
                    if(removeStringInName)
                    {
                        item.Name = item.Name.Replace(desiredStringinName, "");
                    }
                    nodes.Add(item);
                }
            }
            return nodes.ToArray();
        }
    }
}
