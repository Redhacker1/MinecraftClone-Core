using System.Collections.Generic;
using System.Numerics;
using Silk.NET.Assimp;


// Referenced from: http://assimp.sourceforge.net/lib_html/materials.html
namespace Engine.AssetLoading.AssimpIntegration
{
    public struct AssimpMaterialStruct
    {
        public Dictionary<string, ManagedMaterialProperty> MaterialProperty;
    }
}