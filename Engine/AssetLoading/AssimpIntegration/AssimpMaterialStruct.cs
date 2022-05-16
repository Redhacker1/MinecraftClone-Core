using System.Collections.Generic;
using System.Numerics;
using Silk.NET.Assimp;


// Referenced from: http://assimp.sourceforge.net/lib_html/materials.html
namespace Engine.AssetLoading.AssimpIntegration
{
    public struct AssimpMaterialStruct
    {
        public string Name;
        
        public Vector4 Diffuse;
        public Vector4 Ambient;
        public Vector4 Emissive;
        public Vector4 Tranparent;
        public Vector4 Reflective;
        public Vector4 Specular;

        public ShadingMode ShadingModel;

        public bool Wireframe;
        public bool TwoSided;
        public bool Additive;
        
        public float Opacity;
        public float Shininess;
        public float shininessStrength;
        public float ShinePercent;
        public float Refraction_Index;
        public float Reflectivity;
        public float BumpScaling;
        public float DisplacementScaling;
        public float TransparencyFactor;
        
        public Dictionary<TextureType, TextureLayer[]> _textures;
        
    }
}