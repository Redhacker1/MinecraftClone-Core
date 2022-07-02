using System;

namespace Engine.AssetLoading.AssimpIntegration
{
    public class AssimpScene
    {
        bool HasAnimations => Animations == Array.Empty<AssimpAnimation>() || Animations == null;
        bool HasCameras;
        bool HasLights;
        bool HasMaterials => Materials == Array.Empty<AssimpMaterialStruct>() || Materials == null;
        bool HasMeshes => Meshes == Array.Empty<AssimpMesh>() || Meshes == null;
        bool HasTextures;

        public AssimpMaterialStruct[] Materials;
        public AssimpNode RootNode;
        public AssimpMesh[] Meshes;
        public AssimpAnimation[] Animations;
    }
}