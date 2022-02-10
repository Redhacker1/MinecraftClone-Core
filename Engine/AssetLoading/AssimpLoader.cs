﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using Engine.Objects;
using Engine.Renderable;
using Engine.Windowing;
using Silk.NET.Assimp;
using File = System.IO.File;
using Mesh = Engine.Renderable.Mesh;
using Texture = Engine.Rendering.Texture;
using assimp = Silk.NET.Assimp.Assimp;
using PostProcessPreset = Silk.NET.Assimp.PostProcessPreset;
using TextureType = Silk.NET.Assimp.TextureType;

namespace Engine.AssetLoading
{
    public class AssimpLoader
    {
        
        /// <summary>
        /// Loads file from ASSIMP
        /// </summary>
        /// <param name="meshName"></param>
        /// <param name="bindingObject"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <summary>
        public static unsafe (Mesh[],IReadOnlyDictionary<string ,Texture>) LoadMesh(string meshName, GameObject BindingObject, Renderable.Material material)
        {
            Dictionary<string ,Texture> textures = new Dictionary<string ,Texture>();
            //Stride.Core.IO.
            assimp things = assimp.GetApi();
            
            Console.WriteLine(Path.GetExtension(meshName));
            string directory = Path.GetDirectoryName(meshName);
            if (!File.Exists(meshName))
            {
                throw new FileNotFoundException("The specified Model was not found");
            }
            if(things.IsExtensionSupported(Path.GetExtension(meshName)) == 0)
            {
                throw new InvalidOperationException("the extension is unsupported!");
            }
            
            var scene = things.ImportFile(meshName, (uint)PostProcessPreset.TargetRealTimeMaximumQuality);


            Console.WriteLine($"Texture count is {scene->MNumTextures}");
            Console.WriteLine($"Material count is {scene->MNumMaterials}");


            for (int materialIndex = 0; materialIndex < scene->MNumMaterials; materialIndex++)
            {
                var currentMaterial = scene->MMaterials[materialIndex];
                foreach (TextureType type in Enum.GetValues<TextureType>())
                {
                    
                    uint textureCount = things.GetMaterialTextureCount(currentMaterial, type);
                    if (textureCount != 0)
                    {
                        Console.WriteLine($"Material {materialIndex} contains {textureCount} texture(s) of type {type}");
                        
                        //Console.WriteLine("Has Textures!");
                        //List<Texture>
                        //Console.WriteLine($"Material {MaterialIndex}, {type}, {texturetypecount}");

                        for (int textureIndex = 0; textureIndex < textureCount; textureIndex++)
                        {
                            /*var texture = thing.GetMaterialTexture(currentMaterial, TextureType.TextureTypeBaseColor,
                                (uint)textureIndex, Span<AssimpString>.Empty, Span<TextureMapping>.Empty,
                                Span<uint>.Empty, Span<float>.Empty, Span<TextureOp>.Empty, Span<TextureMapMode>.Empty,
                                Span<uint>.Empty); */

                            //var Mat = things.GetMaterialTexture(currentMaterial,type, textureIndex, );

                            //Material materialtest = Material.New();
                        }
                    }
                }
            }




            Mesh[] meshes = new Mesh[scene->MNumMeshes];


            for (int meshcount = 0; meshcount < scene->MNumMeshes; meshcount++)
            {
                var mesh = scene->MMeshes[meshcount];

                uint meshVertCount = scene->MMeshes[meshcount]->MNumVertices;

                var meshUvsptr = scene->MMeshes[meshcount]->MTextureCoords;
                var meshUvsArr = new Vector3[mesh->MNumVertices];
                if (meshUvsptr[0] != null)
                {
                    //Vec3PointerAndOffsetToArray(ref meshUvsArr, meshUvsptr.Element0, meshVertCount);

                    for (int i = 0; i < mesh->MNumVertices; i++)
                    {
                        meshUvsArr[i] = new Vector3(meshUvsptr[0][i].X, meshUvsptr[0][i].Y, meshUvsptr[0][i].Z);
                    }
                }

                var meshvertsptr = mesh->MVertices;
                var meshVertsArr = new Vector3[mesh->MNumVertices];
                if (meshvertsptr != null)
                {
                    //Vec3PointerAndOffsetToArray(ref meshVertsArr, meshvertsptr, meshVertCount); 
                    for (int i = 0; i < mesh->MNumVertices; i++)
                    {
                        meshVertsArr[i] = new Vector3(mesh->MVertices[i].X, mesh->MVertices[i].Y, mesh->MVertices[i].Z);
                    }
                }

                var meshNormalsptr = mesh->MNormals;
                var meshNormalsArr = new Vector3[meshVertCount];
                if (meshNormalsptr != null)
                {
                    for (int i = 0; i < mesh->MNumVertices; i++)
                    {
                        meshNormalsArr[i] = new Vector3(mesh->MNormals[i].X, mesh->MNormals[i].Y, mesh->MNormals[i].Z);
                    }
                }

                var meshTangentsptr = mesh->MTangents;
                var meshTangentsArr = new Vector3[meshVertCount];
                if (meshTangentsptr != null)
                {
                    for (int i = 0; i < mesh->MNumVertices; i++)
                    {
                        meshTangentsArr[i] = new Vector3(mesh->MTangents[i].X, mesh->MTangents[i].Y, mesh->MTangents[i].Z);
                    }
                }

                var meshBiTangentsptr = mesh->MBitangents;
                var meshBiTangentsArr = new Vector3[meshVertCount];
                if (meshBiTangentsptr != null)
                {
                    for (int i = 0; i < mesh->MNumVertices; i++)
                    {
                        meshBiTangentsArr[i] = new Vector3(mesh->MBitangents[i].X, mesh->MBitangents[i].Y, mesh->MBitangents[i].Z);
                    }
                }
                

                // I fully expect this to fail if the integer rolls over, that being said I want to try and preallocate where I can, might wanna look into it later.
                var indicies = new List<uint>((int)mesh->MNumFaces * 3);
                
                for (uint face = 0; face < mesh->MNumFaces; face++)
                {
                    var currentface = mesh->MFaces[face];
                    if (currentface.MNumIndices != 3u)
                    {
                        continue;
                    }

                    indicies.Add(currentface.MIndices[0]);
                    indicies.Add(currentface.MIndices[1]);
                    indicies.Add(currentface.MIndices[2]);
                }
                
                meshes[meshcount] = new Mesh(BindingObject, material);
                MeshData data = new MeshData()
                {
                    _indices = indicies.ToArray(),
                    _vertices = meshVertsArr,
                    _uvs = meshUvsArr
                    
                };
                meshes[meshcount].GenerateMesh(data);
                for (int Material = 0; Material < scene->MMaterials[mesh->MMaterialIndex]->MNumProperties ; Material++)
                {
                    var mat = scene->MMaterials[mesh->MMaterialIndex];

                    foreach (var TexType in Enum.GetValues<TextureType>())
                    {
                        var TexturesInStack = things.GetMaterialTextureCount(mat, TexType);
                        if (TexturesInStack > 0)
                        {

                            for (uint TextureIndex = 0; TextureIndex < TexturesInStack; TextureIndex++)
                            {
                                AssimpString pathData = new AssimpString();
                                uint Flags = 0;
                                var success = things.GetMaterialTexture(mat, TextureType.TextureTypeDiffuse, TextureIndex, ref pathData, null, null, null, null, null, ref Flags);

                                if (success == Return.ReturnFailure)
                                {
                                    Console.WriteLine("Material failure!");
                                }
                                else
                                {
                                    var path = Path.Combine(directory, pathData);
                                    if (textures.ContainsKey(path) == false)
                                    {
                                        textures.Add(path, new Texture(WindowClass._renderer.Device, path));
                                    }
                                }
                            }
                        }
                    }
                    
                }
            }


            things.Dispose();
            return (meshes, textures);
        }
        
    }

}