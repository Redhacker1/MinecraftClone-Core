using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Engine.Objects;
using Silk.NET.Assimp;
using File = System.IO.File;
using Mesh = Engine.Renderable.Mesh;

namespace Engine.AssetLoading
{
    public class AssimpLoader
    {
        
        /// <summary>
        /// Loads file from ASSIMP
        /// </summary>
        /// <param name="MeshName"></param>
        /// <param name="bindingObject"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static unsafe Mesh[] LoadMesh(string MeshName, MinimalObject bindingObject)
        {
            var thing = Assimp.GetApi();
            
            Console.WriteLine(Path.GetExtension(MeshName));

            if ( !File.Exists(MeshName) || thing.IsExtensionSupported(Path.GetExtension(MeshName)) == 0)
            {
                throw new FileNotFoundException(message: "The specified Model was not found or the extension is unsupported!");
            } 
            var pointer = thing.ImportFile(MeshName,(uint)PostProcessPreset.TargetRealTimeMaximumQuality );


            Mesh[] Meshes = new Mesh[pointer->MNumMeshes];
            

            for (int meshcount = 0; meshcount < pointer->MNumMeshes; meshcount++)
            {
                var Mesh = pointer->MMeshes[meshcount];
                uint meshVertCount = pointer->MMeshes[meshcount]->MNumVertices;

                var meshUvsptr = pointer->MMeshes[meshcount]->MTextureCoords;
                var meshUvsArr = new Vector3[meshVertCount];
                if (meshUvsptr.Element0 != null)
                {
                    Vec3PointerAndOffsetToArray(ref meshUvsArr, meshUvsptr.Element0, meshVertCount);
                }               

                var meshvertsptr = pointer->MMeshes[meshcount]->MVertices;
                var meshVertsArr = new Vector3[meshVertCount];
                if (meshvertsptr != null)
                {
                    Vec3PointerAndOffsetToArray(ref meshVertsArr, meshvertsptr, meshVertCount);   
                }

                var meshNormalsptr = pointer->MMeshes[meshcount]->MNormals;
                var meshNormalsArr = new Vector3[meshVertCount];
                if (meshNormalsptr != null)
                {
                    Vec3PointerAndOffsetToArray(ref meshNormalsArr, meshNormalsptr, meshVertCount);   
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

                Meshes[meshcount] = new Mesh(meshVertsArr, meshUvsArr, bindingObject);

                // I fully expect this to fail if the integer rolls over, that being said I want to try and preallocate where I can, might wanna look into it later.
                var Indicies = new List<uint>((int) Mesh->MNumVertices);

                for (int face = 0; face < Mesh->MNumFaces; face++)
                {
                    var currentface = Mesh->MFaces[face];
                    for (int index = 0; index < currentface.MNumIndices; index++)
                    {
                        Indicies.Add(Mesh->MFaces[face].MIndices[index]);
                    }
                }
                

                Meshes[meshcount]._indices = Indicies.ToArray();

            }
            
            

            return Meshes;
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
    
}