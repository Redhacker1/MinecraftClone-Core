using System.Numerics;
using Engine.MathLib;

namespace Engine.AssetLoading.AssimpIntegration
{
    public class AssimpNode
    {
        public string Name;

        public Transform Transform;
        public AssimpNode[] Children;
        public AssimpNode Parent;

        public uint[] MeshIndices;


        public AssimpNode FindNode(string NodeName, out bool Success)
        {
            if (Name != NodeName)
            {
                AssimpNode CurrentNode = null;
                AssimpNode ResultNode = null;
                Success = false;
                foreach (AssimpNode node in Children)
                {
                    CurrentNode = node;
                    ResultNode = CurrentNode.FindNode(NodeName, out Success);

                    if (Success)
                    {
                        break;
                    }

                    ResultNode = null;
                }

                return ResultNode;
            }

            Success = true;
            return this;
        }

    }
}