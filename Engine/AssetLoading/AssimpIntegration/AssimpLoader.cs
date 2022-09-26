using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using Engine.MathLib;
using Silk.NET.Assimp;
using File = System.IO.File;
using Mesh = Silk.NET.Assimp.Mesh;

namespace Engine.AssetLoading.AssimpIntegration
{
    // Mostly hacked together by me: (Donovan Strawhacker)
    public static class AssimpLoader
    {
        // NOTE: Should these exceptions be made return codes? Should they actually throw?
        /// <summary>
        /// Imports a mesh from path, converts it from C ASSIMP data to C# ASSIMP.
        /// Unsupported Features: Embedded Textures, Cameras, flags
        /// Supported Features: Animations, Mesh data, Material data, ASSIMP Tree data., Raw bone data provided by ASSIMP.
        /// Wont be Supported Features: Bone Hierarchies (technically possible in SOME circumstances, but ASSIMP provides no way to retrieve them reliably.)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="Flags"></param>
        /// <returns>An ASSIMP Scene</returns>
        /// <exception cref="FileNotFoundException">The importer determined the file did not exist.</exception>
        /// <exception cref="InvalidOperationException">The path to the file is not supported with ASSIMP</exception>
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
            
            
            Stopwatch stopwatch = Stopwatch.StartNew();
            Scene* scene = APIContext.ImportFile(path, (uint)Flags);

            if (scene == null)
            {
                throw new("General import exception, loading file failed!");
            }
            Console.WriteLine($"Time to load file with ASSIMP: {stopwatch.Elapsed.TotalMilliseconds}");
            stopwatch.Restart();
            
            
            Console.WriteLine($"Texture count is {scene->MNumTextures}");
            Console.WriteLine($"Material count is {scene->MNumMaterials}");
            Console.WriteLine($"Mesh count is {scene->MNumMeshes}");
            AssimpScene assimpScene = new AssimpScene
            {
                Animations = PortAnimations(scene->MAnimations, scene->MNumAnimations),
                // Create The node tree!
                // 100% Complete
                RootNode = GenerateNodeTree(scene->MRootNode),
                // Port the Meshes, skeletons, etc. Need to investigate how this method scales. 
                // 100%
                Meshes = PortMeshes(scene->MMeshes, scene->MNumMeshes),
                // 100% complete, This is handled similarly to ASSIMP, it's quite a lot less friendly than I would have liked, but still better,
                Materials = PortMaterial(scene->MMaterials, scene->MNumMaterials)
            };


            Console.WriteLine($"Time to parse assimp structures: {stopwatch.Elapsed.TotalMilliseconds}");
            APIContext.Dispose();
            return assimpScene;
        }

        static unsafe AssimpAnimation[] PortAnimations(Animation** anims, uint count)
        {
            AssimpAnimation[] ManagedAnimations = new AssimpAnimation[count];

            for (int animIndex = 0; animIndex < count; animIndex++)
            {
                Animation* AssimpAnimation = anims[animIndex];
                ref AssimpAnimation ManagedAnimation = ref ManagedAnimations[animIndex];
                
                ManagedAnimation.Duration = AssimpAnimation->MDuration;
                ManagedAnimation.Name = AssimpAnimation->MName;
                ManagedAnimation.Channel = new AssimpGenericAnimData<Vector3>[AssimpAnimation->MNumChannels];
                ManagedAnimation.MeshChannels = new AssimpGenericAnimData<uint>[AssimpAnimation->MNumMeshChannels];
                ManagedAnimation.MeshMorphChannels = new AssimpGenericAnimData<MeshMorphAnimKey>[AssimpAnimation->MNumMorphMeshChannels];
                
                ManagedAnimation.TicksPerSecond = AssimpAnimation->MTicksPerSecond;
                
                
                for (int Channel = 0; Channel < AssimpAnimation->MNumChannels; Channel++)
                {
                    ref NodeAnim AssimpChannel = ref *AssimpAnimation->MChannels[Channel];
                    ref AssimpGenericAnimData<Vector3> ManagedChannel = ref ManagedAnimation.Channel[animIndex];

                    ManagedChannel.Name = AssimpChannel.MNodeName;
                    ManagedChannel.Keys = new KeyValuePair<double, Vector3>[AssimpChannel.MNumPositionKeys];
                    for (int position = 0; position < AssimpChannel.MNumPositionKeys; position++)
                    {
                        VectorKey key = AssimpChannel.MPositionKeys[position];
                        ManagedChannel.Keys[position] = new(key.MTime, key.MValue);
                    }
                    

                }
                
                for (int Channel = 0; Channel < AssimpAnimation->MNumMeshChannels; Channel++)
                {
                    ref MeshAnim AssimpChannel = ref *AssimpAnimation->MMeshChannels[Channel];
                    ref AssimpGenericAnimData<uint> ManagedChannel = ref ManagedAnimation.MeshChannels[animIndex];

                    ManagedChannel.Name = AssimpChannel.MName;
                    ManagedChannel.Keys = new KeyValuePair<double, uint>[AssimpChannel.MNumKeys];
                    for (int position = 0; position < AssimpChannel.MNumKeys; position++)
                    {
                        MeshKey key = AssimpChannel.MKeys[position];
                        ManagedChannel.Keys[position] = new(key.MTime, key.MValue);
                    }
                }
                
                for (int Channel = 0; Channel < AssimpAnimation->MNumMeshChannels; Channel++)
                {
                    ref MeshAnim AssimpChannel = ref *AssimpAnimation->MMeshChannels[Channel];
                    ref AssimpGenericAnimData<uint> ManagedChannel = ref ManagedAnimation.MeshChannels[animIndex];

                    ManagedChannel.Name = AssimpChannel.MName;
                    ManagedChannel.Keys = new KeyValuePair<double, uint>[AssimpChannel.MNumKeys];
                    for (int position = 0; position < AssimpChannel.MNumKeys; position++)
                    {
                        MeshKey key = AssimpChannel.MKeys[position];
                        ManagedChannel.Keys[position] = new KeyValuePair<double, uint>(key.MTime, key.MValue);
                    }
                }

                for (int Channel = 0; Channel < AssimpAnimation->MNumMorphMeshChannels; Channel++)
                {
                    ref MeshMorphAnim AssimpChannel = ref *AssimpAnimation->MMorphMeshChannels[Channel];
                    ref AssimpGenericAnimData<MeshMorphAnimKey> ManagedChannel = ref ManagedAnimation.MeshMorphChannels[animIndex];

                    ManagedChannel.Name = AssimpChannel.MName;
                    ManagedChannel.Keys = new KeyValuePair<double, MeshMorphAnimKey>[AssimpChannel.MNumKeys];
                    for (int position = 0; position < AssimpChannel.MNumKeys; position++)
                    {
                        MeshMorphKey assimpkey = AssimpChannel.MKeys[position];
                        MeshMorphAnimKey key = new();
                        key.Values = new uint[assimpkey.MNumValuesAndWeights];
                        key.Weights = new double[assimpkey.MNumValuesAndWeights];

                        UnmanagedToManagedCopy(key.Values,
                            new Span<uint>(assimpkey.MValues, (int) assimpkey.MNumValuesAndWeights));
                        
                        UnmanagedToManagedCopy(key.Weights,
                            new Span<double>(assimpkey.MWeights, (int) assimpkey.MNumValuesAndWeights));
                        
                        ManagedChannel.Keys[position] = new KeyValuePair<double, MeshMorphAnimKey>(assimpkey.MTime, key);
                    }
                }
                

                // Port animations here!
            }
            

            return ManagedAnimations;
        }

        static unsafe AssimpMaterialStruct[] PortMaterial(Material** Materials, uint Length)
        {
            AssimpMaterialStruct[] ManagedMaterials = new AssimpMaterialStruct[Length];
            
            for (int Material = 0; Material < Length; Material++)
            {
                Material AssimpMaterial = *Materials[Material];
                Dictionary<string, ManagedMaterialProperty> properties = new();

                for (int materialProperty = 0; materialProperty < AssimpMaterial.MNumProperties; materialProperty++)
                {
                    ManagedMaterialProperty property = new(*AssimpMaterial.MProperties[materialProperty]);
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

                ref AssimpMesh ManagedMesh = ref OutMeshes[MeshIndex];
                ManagedMesh.Name = CurrentMesh.MName;
                ManagedMesh.UVChannels = *CurrentMesh.MNumUVComponents;
                ManagedMesh.Colors = ProcessColorChannels(ref CurrentMesh.MColors, CurrentMesh.MNumVertices).ToArray();
                ManagedMesh.TextureCoords = ProcessUVChannels(ref CurrentMesh.MTextureCoords, CurrentMesh.MNumVertices).ToArray();
                ManagedMesh.MaterialIndex = CurrentMesh.MMaterialIndex;
                ManagedMesh.PrimitiveTypes = (PrimitiveType) CurrentMesh.MPrimitiveTypes;

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
                        ref AssimpBone ManagedBone = ref Bones[CurrentMesh.MNumBones];
                        ManagedBone.Name = CurrentBone.MName;
                        ManagedBone.Weights = new AssimpBone.AssimpVertexWeight[CurrentBone.MNumWeights];
                        ManagedBone.Offset = CurrentBone.MOffsetMatrix;

                        for (int WeightIndex = 0; WeightIndex < CurrentBone.MNumWeights; WeightIndex++)
                        {
                            VertexWeight CurrentWeight = *CurrentBone.MWeights;
                            ManagedBone.Weights[WeightIndex] = new AssimpBone.AssimpVertexWeight
                            {
                                Strength = CurrentWeight.MWeight,
                                VertexIndex = CurrentWeight.MVertexId
                            };
                        }
                        Bones[BoneIndex] = ManagedBone;
                    }

                    ManagedMesh.Bones = Bones;
                }

            }
            return OutMeshes;
        }

        public static unsafe List<Vector3[]> ProcessUVChannels(ref Mesh.MTextureCoordsBuffer texcoords, uint NumVertices)
        {
            List<Vector3[]> uvChannels = new(Assimp.MaxNumberOfTexturecoords);
            if (NumVertices > 0)
            {
                for (int uvChannelIndex = 0; uvChannelIndex < Assimp.MaxNumberOfTexturecoords; uvChannelIndex++)
                {
                    if (texcoords[uvChannelIndex] != null)
                    {
                        uvChannels.Add(new Vector3[NumVertices]);
                        UnmanagedToManagedCopy(texcoords[uvChannelIndex], NumVertices, uvChannels[uvChannelIndex]);   
                    }
                }
            }
            uvChannels.TrimExcess();
            return uvChannels;
        }

        public static unsafe List<Vector4[]> ProcessColorChannels(ref Mesh.MColorsBuffer Colors, uint NumVertices)
        {
            List<Vector4[]> colorChannels = new List<Vector4[]>(Assimp.MaxNumberOfColorSets);
            if (NumVertices > 0)
            {
                for (int colorChannelIndex = 0; colorChannelIndex < Assimp.MaxNumberOfTexturecoords; colorChannelIndex++)
                {
                    if (Colors[colorChannelIndex] != null)
                    {
                        colorChannels.Add(new Vector4[NumVertices]);
                        UnmanagedToManagedCopy(Colors[colorChannelIndex], NumVertices, colorChannels[colorChannelIndex]);   
                    }
                }
            }
            colorChannels.TrimExcess();
            return colorChannels;
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
            AssimpNode CurrentNode = new();
            
            CurrentNode.MeshIndices = Array.Empty<uint>();
            CurrentNode.Children = Array.Empty<AssimpNode>();
            CurrentNode.Name = node->MName;
            
            Transform.Decompose(in node->MTransformation, out Transform transform);
            CurrentNode.Transform = transform;
            

            if (node->MNumMeshes != 0)
            {
                CurrentNode.MeshIndices = new uint[node->MNumMeshes];
                for (int meshIndex = 0; meshIndex < node->MNumMeshes; meshIndex++)
                {
                    {
                        CurrentNode.MeshIndices[meshIndex] = node->MMeshes[meshIndex];                    
                    }

                }

            }
            if (node->MNumChildren != 0)
            {
                CurrentNode.Children = new AssimpNode[node->MNumChildren];
                for (int nodeIndex = 0; nodeIndex < node->MNumChildren; nodeIndex++)
                {
                    {
                        CurrentNode.Children[nodeIndex] = GenerateNodeTree(node->MChildren[nodeIndex]);
                        CurrentNode.Children[nodeIndex].Parent = CurrentNode;   
                    }
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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once UnusedMethodReturnValue.Local
        static unsafe bool UnmanagedToManagedCopy<T>(Span<T> source, Span<T> destination) where T: unmanaged
        {
            fixed (T* sourcevar = source)
            {
                return UnmanagedToManagedCopy(sourcevar, (uint)source.Length, destination);
            }
        }

    }
}