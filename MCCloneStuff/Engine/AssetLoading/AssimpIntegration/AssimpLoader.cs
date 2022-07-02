using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using Silk.NET.Assimp;
using File = System.IO.File;
using Mesh = Silk.NET.Assimp.Mesh;

namespace Engine.AssetLoading.AssimpIntegration
{
    // Mostly hacked together by me: (Donovan Strawhacker)
    public class AssimpLoader
    {
        /// <summary>
        /// Imports a mesh from path, converts it from C ASSIMP data to C# ASSIMP.
        /// Unsupported Features: Animations, Embedded Textures, Cameras, flags
        /// Supported Features: Mesh data, Material data, ASSIMP Tree data., Raw bone data provided by ASSIMP.
        /// Wont be Supported Features: Bone Hierarchies (technically possible in SOME circumstances, but ASSIMP provides no way to retrieve them reliably.)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="Flags"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public static unsafe AssimpScene AssimpImport(string path, PostProcessSteps Flags = PostProcessSteps.None)
        {
            Assimp APIContext = Assimp.GetApi();
            
            string directory = Path.GetDirectoryName(path);
            if (!File.Exists(path) || directory == null)
            {
                throw new FileNotFoundException("The specified Model was not found");
            }
            if(APIContext.IsExtensionSupported(Path.GetExtension(path)) == 0)
            {
                throw new InvalidOperationException("the extension is unsupported!");
            }
            
            Scene* scene = APIContext.ImportFile(path, (uint)Flags);

            if (scene == null)
            {
                throw new Exception("General import exception, loading file failed!");
            }
            
            Console.WriteLine($"Texture count is {scene->MNumTextures}");
            Console.WriteLine($"Material count is {scene->MNumMaterials}");
            Console.WriteLine($"Mesh count is {scene->MNumMeshes}");
            var assimpScene = new AssimpScene();



            assimpScene.Animations = PortAnimations(scene->MAnimations, scene->MNumAnimations);
            
            // 100% Complete
            // Create The node tree!
            assimpScene.RootNode = GenerateNodeTree(scene->MRootNode);
            
            // 100%
            // Port the Meshes
            assimpScene.Meshes = PortMeshes(scene->MMeshes, scene->MNumMeshes);
            
            // 100% complete, This is handled similarly to ASSIMP, it's quite a lot less friendly, but still useful,
            // TODO: Make this more pleasant to work with and not as barebones.
            assimpScene.Materials = PortMaterial(scene->MMaterials, scene->MNumMaterials, APIContext);
            
            APIContext.Dispose();
            return assimpScene;
        }

        static unsafe AssimpAnim[] PortAnimations(Animation** anims, uint count)
        {
            AssimpAnim[] ManagedAnimations = new AssimpAnim[count];

            for (int animIndex = 0; animIndex < count; animIndex++)
            {
                Animation* AssimpAnimation = anims[animIndex];
                ref AssimpAnim ManagedAnimation = ref ManagedAnimations[animIndex];

                // Port animations here!
            }
            

            return ManagedAnimations;
        }

        static unsafe AssimpMaterialStruct[] PortMaterial(Material** Materials, uint Length, Assimp Context)
        {
            AssimpMaterialStruct[] ManagedMaterials = new AssimpMaterialStruct[Length];
            
            for (int Material = 0; Material < Length; Material++)
            {
                Material AssimpMaterial = *Materials[Material];
                Dictionary<string, ManagedMaterialProperty> properties = new Dictionary<string, ManagedMaterialProperty>();

                for (int materialProperty = 0; materialProperty < AssimpMaterial.MNumProperties; materialProperty++)
                {
                    ManagedMaterialProperty property = new ManagedMaterialProperty(*AssimpMaterial.MProperties[materialProperty]);
                    properties[property.Key] = property;
                }

                ManagedMaterials[Material].MaterialProperty = properties;
            }

            return ManagedMaterials;
        }

        static unsafe AssimpMesh[] PortMeshes(Mesh** meshStart, uint Length)
        {
            AssimpMesh[] OutMeshes = new AssimpMesh[Length];

            for (int MeshIndex = 0; MeshIndex < Length; MeshIndex++)
            {
                Mesh CurrentMesh = *meshStart[MeshIndex];
                
                AssimpMesh ManagedMesh = new AssimpMesh
                {
                    Name = CurrentMesh.MName,
                    UVChannels = *CurrentMesh.MNumUVComponents,
                    Colors = ProcessColorChannels(CurrentMesh).ToArray(),
                    TextureCoords = ProcessUVChannels(CurrentMesh).ToArray(),
                    MaterialIndex = CurrentMesh.MMaterialIndex,
                    PrimitiveTypes = (PrimitiveType)CurrentMesh.MPrimitiveTypes
                    
                };
                
                if (CurrentMesh.MTangents != null && CurrentMesh.MNumVertices > 0)
                {
                    ManagedMesh.Tangents = new Vector3[CurrentMesh.MNumVertices];
                    UnmanagedToManagedCopy(CurrentMesh.MTangents, CurrentMesh.MNumVertices, ManagedMesh.Tangents);
                    ManagedMesh.BiTangents = new Vector3[CurrentMesh.MNumVertices];
                    UnmanagedToManagedCopy(CurrentMesh.MBitangents, CurrentMesh.MNumVertices, ManagedMesh.BiTangents);
                }
                
                if (CurrentMesh.MVertices != null && CurrentMesh.MNumVertices > 0)
                {
                    ManagedMesh.Vertices = new Vector3[CurrentMesh.MNumVertices];
                    UnmanagedToManagedCopy(CurrentMesh.MVertices, CurrentMesh.MNumVertices, ManagedMesh.Vertices);
                    ManagedMesh.HasPositions = true;
                }
                if (CurrentMesh.MNormals != null && CurrentMesh.MNumVertices > 0)
                {
                    ManagedMesh.Normals = new Vector3[CurrentMesh.MNumVertices];
                    UnmanagedToManagedCopy(CurrentMesh.MVertices, CurrentMesh.MNumVertices, ManagedMesh.Vertices);
                    ManagedMesh.HasNormals = true;
                }
                if (CurrentMesh.MFaces != null && CurrentMesh.MNumFaces > 0)
                {
                    ManagedMesh.Indices = new uint[CurrentMesh.MNumFaces][];

                    for (int FaceIndex = 0; FaceIndex < CurrentMesh.MNumFaces; FaceIndex++)
                    {
                        Face CurrentFace = CurrentMesh.MFaces[FaceIndex];

                        ManagedMesh.Indices[FaceIndex] = new uint[CurrentFace.MNumIndices];
                        UnmanagedToManagedCopy(CurrentFace.MIndices, CurrentFace.MNumIndices,
                            ManagedMesh.Indices[FaceIndex]);
                    }
                    ManagedMesh.HasFaces = true;
                }

                if (CurrentMesh.MNumBones > 0&& CurrentMesh.MBones != null)
                {
                    ManagedMesh.HasBones = true;
                    AssimpBone[] Bones = new AssimpBone[CurrentMesh.MNumBones];
                    for (int BoneIndex = 0; BoneIndex < CurrentMesh.MNumBones; BoneIndex++)
                    {
                        Bone CurrentBone = *CurrentMesh.MBones[BoneIndex];
                        AssimpBone ManagedBone = new AssimpBone()
                        {
                            Name = CurrentBone.MName,
                            Weights = new AssimpBone.AssimpVertexWeight[CurrentBone.MNumWeights],
                            Offset = CurrentBone.MOffsetMatrix
                        };

                        for (int WeightIndex = 0; WeightIndex < CurrentBone.MNumWeights; WeightIndex++)
                        {
                            VertexWeight CurrentWeight = *CurrentBone.MWeights;
                            ManagedBone.Weights[WeightIndex] = new AssimpBone.AssimpVertexWeight()
                            {
                                Strength = CurrentWeight.MWeight,
                                VertexIndex = CurrentWeight.MVertexId
                            };
                        }
                        Bones[BoneIndex] = ManagedBone;
                    }

                    ManagedMesh.Bones = Bones;
                }
                
                
                OutMeshes[MeshIndex] = ManagedMesh;
                
            }
            return OutMeshes;
        }

        public static unsafe List<Vector3[]> ProcessUVChannels(Mesh mesh)
        {
            List<Vector3[]> uvChannels = new List<Vector3[]>();

            for (int uvChannelIndex = 0; uvChannelIndex < 8; uvChannelIndex++)
            {
                if (mesh.MColors[uvChannelIndex] != null || mesh.MNumVertices > 0)
                {
                    uvChannels.Add(new Vector3[mesh.MNumVertices]);
                    UnmanagedToManagedCopy(mesh.MTextureCoords[uvChannelIndex], mesh.MNumVertices,
                        uvChannels[uvChannelIndex]);
                }
                else
                {
                    break;
                }
            }
            return uvChannels;
        }

        public static unsafe List<Vector4[]> ProcessColorChannels(Mesh mesh)
        {
            List<Vector4[]> ColorChannels = new List<Vector4[]>();

            for (int ColorChannelIndex = 0; ColorChannelIndex < 8; ColorChannelIndex++)
            {
                if (mesh.MColors[ColorChannelIndex] != null || mesh.MNumVertices > 0)
                {
                    ColorChannels.Add(new Vector4[mesh.MNumVertices]);
                    UnmanagedToManagedCopy(mesh.MColors[ColorChannelIndex], mesh.MNumVertices,
                        ColorChannels[ColorChannelIndex]);
                }
                else
                {
                    break;
                }
            }
            return ColorChannels;
        }
        
        /// <summary>
        /// Converts the ASSIMP Node Tree struct, into a C#(ified) node tree hierarchy. Only do this to the root node
        /// as parent information above the supplied node is lost unless manually added later!
        /// 
        /// </summary>
        /// <param name="node">The node to generate and propagate to the next children with</param>
        /// <returns>A node with all it's children Converted to a node Tree</returns>
        static unsafe AssimpNode GenerateNodeTree(Node* node)
        {
            AssimpNode CurrentNode = new AssimpNode();
            
            CurrentNode.MeshIndices = Array.Empty<uint>();
            CurrentNode.Children = Array.Empty<AssimpNode>();
            CurrentNode.Name = node->MName;
            CurrentNode.Transform = node->MTransformation;

            if (node->MNumMeshes != 0)
            {
                CurrentNode.MeshIndices = new uint[node->MNumMeshes];
                for (int meshIndex = 0; meshIndex < node->MNumMeshes; meshIndex++)
                {
                    CurrentNode.MeshIndices[meshIndex] = node->MMeshes[meshIndex];
                }

            }
            
            
            if (node->MNumChildren != 0)
            {
                CurrentNode.Children = new AssimpNode[node->MNumChildren];
                for (int nodeIndex = 0; nodeIndex < node->MNumChildren; nodeIndex++)
                {
                    CurrentNode.Children[nodeIndex] = GenerateNodeTree(node->MChildren[nodeIndex]);
                    CurrentNode.Children[nodeIndex].Parent = CurrentNode;
                }
            }
            

            return CurrentNode;
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe bool UnmanagedToManagedCopy<T>(T* source, uint length, Span<T> destination) where T: unmanaged
        {
            if (destination.Length <= length && source != null)
            {
                fixed (void* pDest = destination)
                {
                    Unsafe.CopyBlock(pDest, source, (uint) (length * sizeof(T)));
                }
                return true;
            }
            return false;
        }

    }
}