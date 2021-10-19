using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Numerics;
using Assimp;
using Engine.Objects;
using Silk.NET.Assimp;
using File = System.IO.File;
using Mesh = Engine.Renderable.Mesh;
using Texture = Engine.Rendering.Texture;
using assimp = Silk.NET.Assimp.Assimp;
using PostProcessPreset = Silk.NET.Assimp.PostProcessPreset;
using PrimitiveType = Assimp.PrimitiveType;
using TextureMapping = Silk.NET.Assimp.TextureMapping;
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
        public static unsafe Mesh[] LoadMesh(string meshName, MinimalObject bindingObject)
        {

            AssimpContext things = new AssimpContext();

            var thing = assimp.GetApi();
            
            Console.WriteLine(Path.GetExtension(meshName));

            if ( !File.Exists(meshName) || thing.IsExtensionSupported(Path.GetExtension(meshName)) == 0)
            {
                throw new FileNotFoundException(message: "The specified Model was not found or the extension is unsupported!");
            } 
            var pointer = thing.ImportFile(meshName,(uint)PostProcessPreset.TargetRealTimeMaximumQuality );
            var scene = things.ImportFile(meshName, Assimp.PostProcessPreset.TargetRealTimeMaximumQuality);
            

            Console.WriteLine($"Texture count is {scene.HasTextures}");


            for (int MaterialIndex = 0; MaterialIndex < pointer->MNumMaterials; MaterialIndex++)
            {
                var currentMaterial = pointer->MMaterials[MaterialIndex];
                foreach (TextureType type in Enum.GetValues<TextureType>())
                {
                    uint texturetypecount = thing.GetMaterialTextureCount(currentMaterial, type);
                    
                    if (texturetypecount > 0)
                    {
                        //List<Texture>
                        //Console.WriteLine($"Material {MaterialIndex}, {type}, {texturetypecount}");

                        for (int textureIndex = 0; textureIndex <= texturetypecount; textureIndex++)
                        {
                            var texture = thing.GetMaterialTexture(currentMaterial, TextureType.TextureTypeBaseColor, (uint)textureIndex, Span<AssimpString>.Empty, Span<TextureMapping>.Empty, Span<uint>.Empty, Span<float>.Empty, Span<TextureOp>.Empty, Span<TextureMapMode>.Empty, Span<uint>.Empty);
                        }
                    }
                }
            }
            


            
            Mesh[] meshes = new Mesh[pointer->MNumMeshes];
            

            for (int meshcount = 0; meshcount < pointer->MNumMeshes; meshcount++)
            {
                var mesh = pointer->MMeshes[meshcount];


                    

                uint meshVertCount = pointer->MMeshes[meshcount]->MNumVertices;


                
               
                var meshUvsptr = pointer->MMeshes[meshcount]->MTextureCoords[0];
                var meshUvsArr = new Vector3[mesh->MNumVertices];
                if (meshUvsptr != null)
                {
                    //Vec3PointerAndOffsetToArray(ref meshUvsArr, meshUvsptr.Element0, meshVertCount);

                    for (int i = 0; i < mesh->MNumVertices; i++)
                    {
                        meshUvsArr[i] = new Vector3(meshUvsptr[i].X, meshUvsptr[i].Y, meshUvsptr[i].Z);
                    }
                }               

                var meshvertsptr = pointer->MMeshes[meshcount]->MVertices;
                var meshVertsArr = new Vector3[mesh->MNumVertices];
                if (meshvertsptr != null)
                {
                    //Vec3PointerAndOffsetToArray(ref meshVertsArr, meshvertsptr, meshVertCount); 
                    for (int i = 0; i < mesh->MNumVertices; i++)
                    {
                        meshVertsArr[i] = new Vector3(mesh->MVertices[i].X, mesh->MVertices[i].Y, mesh->MVertices[i].Z);
                    }
                }

                var meshNormalsptr = pointer->MMeshes[meshcount]->MNormals;
                var meshNormalsArr = new Vector3[meshVertCount];
                if (meshNormalsptr != null)
                {
                    for (int i = 0; i < mesh->MNumVertices; i++)
                    {
                        meshNormalsArr[i] = new Vector3(mesh->MNormals[i].X, mesh->MNormals[i].Y,mesh->MNormals[i].Z);
                    }
                }

                var meshTangentsptr = pointer->MMeshes[meshcount]->MTangents;
                var meshTangentsArr = new Vector3[meshVertCount];
                if (meshTangentsptr != null)
                {
                    Vec3PointerAndOffsetToArray(ref meshTangentsArr, meshTangentsptr, meshVertCount);   
                }

                var meshBiTangentsptr = pointer->MMeshes[meshcount]->MTangents;
                var meshBiTangentsArr = new Vector3[meshVertCount];
                if (meshBiTangentsptr != null)
                {
                    Vec3PointerAndOffsetToArray(ref meshBiTangentsArr, meshBiTangentsptr, meshVertCount);
                }


                meshes[meshcount] = new Mesh(meshVertsArr.Reverse().ToArray(), meshUvsArr.Reverse().ToArray(), bindingObject);

                // I fully expect this to fail if the integer rolls over, that being said I want to try and preallocate where I can, might wanna look into it later.
                var indicies = new List<uint>((int) mesh->MNumVertices);
                

                for (int face = 0; face < mesh->MNumFaces; face++)
                {
                    var currentface = mesh->MFaces[face];
                    for (int index = 0; index < currentface.MNumIndices; index++)
                    {
                        indicies.Add((uint)currentface.MIndices[index]);
                        
                    }
                }
                
                meshes[meshcount]._indices = indicies.ToArray(); //mesh1.GetUnsignedIndices();

            }
            
            

            return meshes;
        }
        
        
        private static unsafe void Vec3PointerAndOffsetToArray(ref Vector3[] destination, Vector3* item, uint Count )
        {
            if (destination == null)
            {
                throw new NullReferenceException("Destination is null!");
            }
            for (int i = 0; i < Count; i++)
            {
                destination[i] = item[i];
            }
        }
    }

    public class Material
    {
        Dictionary<TextureType, List<Texture>> textures = new Dictionary<TextureType, List<Texture>>();
    }
    
}