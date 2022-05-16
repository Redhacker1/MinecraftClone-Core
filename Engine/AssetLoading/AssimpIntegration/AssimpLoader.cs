using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using Engine.Renderable;
using Engine.Windowing;
using Silk.NET.Assimp;
using File = System.IO.File;
using Mesh = Silk.NET.Assimp.Mesh;
using Texture = Engine.Rendering.Texture;

namespace Engine.AssetLoading.AssimpIntegration
{
    // Mostly hacked together by me: (Donovan Strawhacker)
    public class AssimpLoader
    {
        /// <summary>
        /// Imports a mesh from 
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


            // 100% Complete
            // Create The node tree!
            assimpScene.RootNode = GenerateNodeTree(scene->MRootNode);
            
            // 70% Complete (Missing Bones support among other smaller things)
            assimpScene.Meshes = PortMeshes(scene->MMeshes, scene->MNumMeshes);
            
            // 80% complete, cannot handle custom material keys. Materials TODO: This method is shit, do better,
            assimpScene.Materials = PortMaterial(scene->MMaterials, scene->MNumMaterials, APIContext, directory);
            
            // Materials





            APIContext.Dispose();
            return assimpScene;


        }

        static unsafe AssimpMaterialStruct[] PortMaterial(Material** Materials, uint Length, Assimp Context, string directory)
        {
            AssimpMaterialStruct[] ManagedMaterials = new AssimpMaterialStruct[Length];
            
            for (int Material = 0; Material < Length; Material++)
            {
                Material AssimpMaterial = *Materials[Material];

                for (int propertyIndex = 0; propertyIndex < AssimpMaterial.MNumProperties; propertyIndex++)
                {
                    MaterialProperty* property =  AssimpMaterial.MProperties[propertyIndex];
                    string key =  AssimpMaterial.MProperties[propertyIndex]->MKey.AsString;
                    
                    
                    
                    uint oneunit = 1u;
                    if (key == "?mat.name")
                    {
                        AssimpString name = new AssimpString();
                        Context.GetMaterialString(AssimpMaterial, "?mat.name", 0, 0, ref name);
                        ManagedMaterials[Material].Name = name;
                    }
                    else if (key.StartsWith("$clr."))
                    {
                        if (key.EndsWith("diffuse"))
                        {
                            Context.GetMaterialColor(AssimpMaterial, "$clr.diffuse", 0, 0, ref ManagedMaterials[Material].Diffuse);
                        }
                        else if (key.EndsWith("emissive"))
                        {
                            Context.GetMaterialColor(AssimpMaterial, "$clr.emissive", 0, 0, ref ManagedMaterials[Material].Emissive);
                        }
                        else if (key.EndsWith("ambient"))
                        {
                            Context.GetMaterialColor(AssimpMaterial, "$clr.ambient", 0, 0, ref ManagedMaterials[Material].Ambient);
                        }
                        else if (key.EndsWith("transparent"))
                        {
                            Context.GetMaterialColor(AssimpMaterial, "$clr.transparent", 0, 0, ref ManagedMaterials[Material].Tranparent);
                        }
                        else if (key.EndsWith("specular"))
                        {
                            Context.GetMaterialColor(AssimpMaterial, "$clr.specular", 0, 0, ref ManagedMaterials[Material].Specular);
                        }
                        else if (key.EndsWith("reflective"))
                        {
                            
                            Context.GetMaterialColor(AssimpMaterial, "$clr.specular", 0, 0, ref ManagedMaterials[Material].Reflective);
                        }
                        else
                        {
                            Console.WriteLine($"Unrecognized/Not supported key {key}");
                        }
                    }
                    else if (key.StartsWith("$mat."))
                    {
                        if (key.EndsWith("shininess"))
                        {
                            Context.GetMaterialFloatArray(AssimpMaterial, "$mat.shininess", 0u, 0u, ref ManagedMaterials[Material].Shininess, ref oneunit);
                        }
                        else if (key.EndsWith("transparencyfactor"))
                        {
                            Context.GetMaterialFloatArray(AssimpMaterial, "$mat.transparencyfactor", 0u, 0u, ref ManagedMaterials[Material].TransparencyFactor, ref oneunit);
                        }
                        else if (key.EndsWith("opacity"))
                        {
                            Context.GetMaterialFloatArray(AssimpMaterial, "$mat.opacity", 0u, 0u, ref ManagedMaterials[Material].Opacity, ref oneunit);
                        }
                        else if (key.EndsWith("refracti"))
                        {
                            Context.GetMaterialFloatArray(AssimpMaterial, "$mat.refracti", 0u, 0u, ref ManagedMaterials[Material].Refraction_Index, ref oneunit);
                        }
                        else if (key.EndsWith("shadingm"))
                        {
                            int ShadingModel = 0;
                            Context.GetMaterialIntegerArray(AssimpMaterial, "$mat.shadingm", 0u, 0u, ref ShadingModel, ref oneunit);
                            ManagedMaterials[Material].ShadingModel = (ShadingMode) ShadingModel;
                        }
                        else if(key.EndsWith("shinpercent"))
                        {
                            Context.GetMaterialFloatArray(AssimpMaterial, "$mat.shinpercent", 0u, 0u, ref ManagedMaterials[Material].ShinePercent, ref oneunit);
                        }
                        else if(key.EndsWith("reflectivity"))
                        {
                            Context.GetMaterialFloatArray(AssimpMaterial, "$mat.reflectivity", 0u, 0u, ref ManagedMaterials[Material].Reflectivity, ref oneunit);
                        }
                        else if(key.EndsWith("bumpscaling"))
                        { 
                            Context.GetMaterialFloatArray(AssimpMaterial, "$mat.bumpscaling", 0u, 0u, ref ManagedMaterials[Material].BumpScaling, ref oneunit);
                        }
                        else if(key.EndsWith("displacementscaling"))
                        {
                            Context.GetMaterialFloatArray(AssimpMaterial, "$mat.displacementscaling", 0u, 0u, ref ManagedMaterials[Material].DisplacementScaling, ref oneunit);
                        }
                        else
                        {
                            Console.WriteLine($"Unrecognized/Not supported key {key}, type is: {property->MType}");
                        }
                        
                    }

                }

                Dictionary<TextureType, TextureLayer[]> textureLayers = new Dictionary<TextureType, TextureLayer[]>();
                foreach (TextureType TexType in Enum.GetValues<TextureType>())
                {
                    uint TexturesInStack = Context.GetMaterialTextureCount(AssimpMaterial, TexType);
                    TextureLayer[] TextureStack =  new TextureLayer[TexturesInStack];
                    textureLayers[TexType] = TextureStack;
                    

                    if (TexturesInStack > 0)
                    {

                        for (uint TextureIndex = 0; TextureIndex < TexturesInStack; TextureIndex++)
                        {
                            AssimpString pathData = new AssimpString();
                            uint Flags = 0;
                            TextureMapping mappingmode = TextureMapping.TextureMappingUV;
                            
                            Context.GetMaterialTexture(
                                AssimpMaterial, 
                                TexType,
                                TextureIndex,
                                ref pathData,
                                ref mappingmode,
                                ref TextureStack[TextureIndex].UVWSource,
                                ref TextureStack[TextureIndex].BlendFactor,
                                ref TextureStack[TextureIndex].TextureOp,
                                ref TextureStack[TextureIndex].MappingModeU,
                                ref Flags
                            );
                            TextureStack[TextureIndex].ProjectionMode = mappingmode;
                            TextureStack[TextureIndex].TextureFlags = (TextureFlags) Flags;
                            TextureStack[TextureIndex].path = pathData;
                        }
                    }
                }
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

                if (CurrentMesh.MNumBones > 0)
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
                            ManagedBone.Weights[WeightIndex] = new AssimpBone.AssimpVertexWeight()
                            {
                                Strength = CurrentBone.MWeights[WeightIndex].MWeight,
                                VertexIndex = CurrentBone.MWeights[WeightIndex].MVertexId
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


        /// <summary>
        /// Converts the ASSIMP Node Tree struct, into a C#(ified) node tree hierarchy. Only do this to the root node
        /// as parent information above the supplied node is lost unless manually added later!
        /// </summary>
        /// <param name="node">The node to generate and propagate to the next children with</param>
        /// <returns>A node with all it's children Converted to a node Tree</returns>
        static unsafe AssimpNode GenerateNodeTree(Node* node)
        {
            Console.WriteLine(node->MName);
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

    }

}