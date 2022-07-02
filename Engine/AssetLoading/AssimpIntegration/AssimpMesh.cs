using System.Numerics;
using Silk.NET.Assimp;

namespace Engine.AssetLoading.AssimpIntegration
{
    public struct AssimpMesh
    {
        public string Name;
        public PrimitiveType PrimitiveTypes;
        
        public uint MaterialIndex;
        public uint ColorChannels;
        public uint UVChannels;

        public bool HasBones;
        public bool HasFaces;
        public bool HasNormals;
        public bool HasPositions;
        public bool HasVertexColors;
        

        public Vector3[] Vertices;
        public Vector3[][] TextureCoords;
        public Vector3[] Tangents;
        public Vector3[] BiTangents;
        public Vector3[] Normals;

        public Vector4[][] Colors;
        public uint[][] Indices;
        public AssimpBone[] Bones;
    }
}