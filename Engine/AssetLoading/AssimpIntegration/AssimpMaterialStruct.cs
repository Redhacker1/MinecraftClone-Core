using System.Collections.Generic;


// Referenced from: http://assimp.sourceforge.net/lib_html/materials.html
namespace Engine.AssetLoading.AssimpIntegration
{
    public struct AssimpMaterialStruct
    {
        public Dictionary<string, ManagedMaterialProperty> MaterialProperty;
    }
}