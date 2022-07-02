namespace Engine.AssetLoading.AssimpIntegration
{
    public class AssimpScene
    {
        bool HasAnims;
        bool HasCameras;
        bool HasLights;
        bool HasMaterials;
        bool HasMeshes;
        bool HasTextures;

        public AssimpMaterialStruct[] Materials;
        public AssimpNode RootNode;
        public AssimpMesh[] Meshes;
        public AssimpAnim[] Animations;
    }
}