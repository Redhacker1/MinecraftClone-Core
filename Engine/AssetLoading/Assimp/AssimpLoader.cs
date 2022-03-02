using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Engine.MathLib;
using Engine.Renderable;
using Engine.Windowing;
using Silk.NET.Assimp;
using assimp = Silk.NET.Assimp.Assimp;
using File = System.IO.File;
using Texture = Engine.Rendering.Texture;

namespace Engine.AssetLoading.Assimp
{
    // Mostly hacked together by me: (Donovan Strawhacker)
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
        public static unsafe (AssimpMeshBuilder[], AssimpMaterialStruct[]) LoadMesh(string meshName)
        {
            
            //Stride.Core.IO.
            assimp things = assimp.GetApi();
            
            string directory = Path.GetDirectoryName(meshName);
            if (!File.Exists(meshName) || directory == null)
            {
                throw new FileNotFoundException("The specified Model was not found");
            }
            if(things.IsExtensionSupported(Path.GetExtension(meshName)) == 0)
            {
                throw new InvalidOperationException("the extension is unsupported!");
            }
            
            var scene = things.ImportFile(meshName, (uint)PostProcessPreset.TargetRealTimeMaximumQuality);

            if (scene == null)
            {
                throw new Exception("General import exception, loading file failed!");
            }



            Console.WriteLine($"Texture count is {scene->MNumTextures}");
            Console.WriteLine($"Material count is {scene->MNumMaterials}");


            scene->MRootNode->MTransformation = Matrix4x4.CreateFromYawPitchRoll(0, MathHelper.DegreesToRadians(90), 0);
            AssimpMaterialStruct[] Materials = new AssimpMaterialStruct[scene->MNumMaterials];
            Dictionary<string, Texture> textures = new Dictionary<string, Texture>();
            for (int Material = 0; Material < scene->MNumMaterials; Material++)
            {
                Material* AssimpMaterial = scene->MMaterials[Material];

                for (int propertyIndex = 0; propertyIndex < AssimpMaterial->MNumProperties; propertyIndex++)
                {
                    MaterialProperty* property = AssimpMaterial->MProperties[propertyIndex];
                    string key = AssimpMaterial->MProperties[propertyIndex]->MKey.AsString;
                    uint oneunit = 1u;
                    Return result;
                    if (key == "?mat.name")
                    {
                        var name = new AssimpString();
                        things.GetMaterialString(AssimpMaterial, "?mat.name", 0, 0, ref name);
                        Materials[Material].Name = name;
                    }
                    else if (key.StartsWith("$clr."))
                    {
                        if (key.EndsWith("diffuse"))
                        {
                            things.GetMaterialColor(AssimpMaterial, "$clr.diffuse", 0, 0, ref Materials[Material].Diffuse);
                        }
                        else if (key.EndsWith("emissive"))
                        {
                            things.GetMaterialColor(AssimpMaterial, "$clr.emissive", 0, 0, ref Materials[Material].Emissive);
                        }
                        else if (key.EndsWith("ambient"))
                        {
                            things.GetMaterialColor(AssimpMaterial, "$clr.ambient", 0, 0, ref Materials[Material].Ambient);
                        }
                        else if (key.EndsWith("transparent"))
                        {
                            things.GetMaterialColor(AssimpMaterial, "$clr.transparent", 0, 0, ref Materials[Material].Tranparent);
                        }
                        else if (key.EndsWith("specular"))
                        {
                            things.GetMaterialColor(AssimpMaterial, "$clr.specular", 0, 0, ref Materials[Material].Specular);
                        }
                        else if (key.EndsWith("reflective"))
                        {
                            
                            things.GetMaterialColor(AssimpMaterial, "$clr.specular", 0, 0, ref Materials[Material].Reflective);
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
                            things.GetMaterialFloatArray(AssimpMaterial, "$mat.shininess", 0u, 0u, ref Materials[Material].Shininess, ref oneunit);
                        }
                        else if (key.EndsWith("transparencyfactor"))
                        {
                            things.GetMaterialFloatArray(AssimpMaterial, "$mat.transparencyfactor", 0u, 0u, ref Materials[Material].TransparencyFactor, ref oneunit);
                        }
                        else if (key.EndsWith("opacity"))
                        {
                            things.GetMaterialFloatArray(AssimpMaterial, "$mat.opacity", 0u, 0u, ref Materials[Material].Opacity, ref oneunit);
                        }
                        else if (key.EndsWith("refracti"))
                        {
                            things.GetMaterialFloatArray(AssimpMaterial, "$mat.refracti", 0u, 0u, ref Materials[Material].Refraction_Index, ref oneunit);
                        }
                        else if (key.EndsWith("shadingm"))
                        {
                            int ShadingModel = 0;
                            things.GetMaterialIntegerArray(AssimpMaterial, "$mat.shadingm", 0u, 0u, ref ShadingModel, ref oneunit);
                            Materials[Material].ShadingModel = (ShadingMode) ShadingModel;
                        }
                        else if(key.EndsWith("shinpercent"))
                        {
                            things.GetMaterialFloatArray(AssimpMaterial, "$mat.shinpercent", 0u, 0u, ref Materials[Material].ShinePercent, ref oneunit);
                        }
                        else if(key.EndsWith("reflectivity"))
                        {
                            things.GetMaterialFloatArray(AssimpMaterial, "$mat.reflectivity", 0u, 0u, ref Materials[Material].Reflectivity, ref oneunit);
                        }
                        else if(key.EndsWith("bumpscaling"))
                        { 
                            things.GetMaterialFloatArray(AssimpMaterial, "$mat.bumpscaling", 0u, 0u, ref Materials[Material].BumpScaling, ref oneunit);
                        }
                        else if(key.EndsWith("displacementscaling"))
                        {
                            things.GetMaterialFloatArray(AssimpMaterial, "$mat.displacementscaling", 0u, 0u, ref Materials[Material].DisplacementScaling, ref oneunit);
                        }
                        else
                        {
                            Console.WriteLine($"Unrecognized/Not supported key {key}, type is: {property->MType}");
                        }
                        
                    }

                }

                Dictionary<TextureType, TextureLayer[]> textureLayers = new Dictionary<TextureType, TextureLayer[]>();
                foreach (var TexType in Enum.GetValues<TextureType>())
                {
                    var TexturesInStack = things.GetMaterialTextureCount(AssimpMaterial, TexType);
                    TextureLayer[] TextureStack =  new TextureLayer[TexturesInStack];
                    textureLayers[TexType] = TextureStack;

                    if (TexturesInStack > 0)
                    {

                        for (uint TextureIndex = 0; TextureIndex < TexturesInStack; TextureIndex++)
                        {
                            AssimpString pathData = new AssimpString();
                            uint Flags = 0;

                            float blendval = 0f;
                            Return success = things.GetMaterialTexture(AssimpMaterial, TextureType.TextureTypeDiffuse, TextureIndex, ref pathData, null,null, ref blendval, ref TextureStack[TextureIndex].TextureOp, null, ref Flags);
                            TextureStack[TextureIndex].Additive = blendval > 0;

                            if (success == Return.ReturnFailure)
                            {
                                Console.WriteLine("Material access failure!");
                            }
                            else
                            {
                                string path = Path.Combine(directory, pathData);
                                TextureStack[TextureIndex].path = path;
                                if (textures.ContainsKey(path) == false)
                                {
                                    textures.Add(path, new Texture(WindowClass._renderer.Device, path, flipY: true));
                                }
                                TextureStack[TextureIndex]._texture = textures[path];
                            }
                        }
                    }
                }

                Materials[Material]._textures = textureLayers;
            }

            AssimpMeshBuilder[] AssimpMeshes = new AssimpMeshBuilder[scene->MNumMeshes];
            for (int meshcount = 0; meshcount < scene->MNumMeshes; meshcount++)
            {
                var mesh = scene->MMeshes[meshcount];

                uint meshVertCount = scene->MMeshes[meshcount]->MNumVertices;

                var meshUvsptr = scene->MMeshes[meshcount]->MTextureCoords;
                var meshUvsArr = new Vector3[mesh->MNumVertices];
                if (meshUvsptr[0] != null)
                {
                    for (int i = 0; i < mesh->MNumVertices; i++)
                    {
                        meshUvsArr[i] = new Vector3(meshUvsptr[0][i].X, meshUvsptr[0][i].Y, meshUvsptr[0][i].Z);
                    }
                }

                var meshvertsptr = mesh->MVertices;
                var meshVertsArr = new Vector3[mesh->MNumVertices];
                if (meshvertsptr != null)
                {
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
                
                MeshData data = new MeshData
                {
                    _indices = indicies.ToArray(),
                    _vertices = meshVertsArr,
                    _uvs = meshUvsArr
                };
                AssimpMeshes[meshcount] = new AssimpMeshBuilder
                {
                    Data = data,
                    MaterialIndex = mesh->MMaterialIndex
                };
            }


            things.Dispose();
            return (AssimpMeshes, Materials);
        }

    }

}