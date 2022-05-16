using System.Numerics;

namespace Engine.AssetLoading.AssimpIntegration;

public struct AssimpBone
{
    
    AssimpBone[] Children;
    public string Name;
    public AssimpVertexWeight[] Weights;
    public Matrix4x4 Offset;

    public struct AssimpVertexWeight
    {
        public uint VertexIndex;
        public float Strength;
    }
    
    
    
}