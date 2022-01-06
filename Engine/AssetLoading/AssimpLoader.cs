using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Engine.Objects;
using Engine.Renderable;
using Silk.NET.Assimp;
using Silk.NET.Core.Contexts;
using File = System.IO.File;

namespace Engine.AssetLoading
{
    public class AssimpLoader
    {
        public static unsafe MeshData[] LoadMesh(string meshName, MinimalObject bindingObject)
        {
            Assimp thing = Assimp.GetApi();

            if (!File.Exists(meshName))
            {
                throw new FileNotFoundException(message: "The specified Model was not found!");
            } 
            Scene* pointer = thing.ImportFile(meshName, 0);

            if (pointer != null)
            {
                throw new NullReferenceException("Mesh failed to import");
            }
            
            
            MeshData[] meshes = new MeshData[pointer->MNumMeshes];
            

            for (int meshcount = 0; meshcount < pointer->MNumMeshes; meshcount++)
            {
                uint meshVertCount = pointer->MMeshes[meshcount]->MNumVertices;

                Mesh.MTextureCoordsBuffer meshUvsptr = pointer->MMeshes[meshcount]->MTextureCoords;
                Vector3[] meshUvsArr = new Vector3[meshVertCount];
                Vec3PointerAndOffsetToArray(ref meshUvsArr, meshUvsptr.Element0, meshVertCount);

                Vector3* meshvertsptr = pointer->MMeshes[meshcount]->MVertices;
                Vector3[] meshVertsArr = new Vector3[meshVertCount];
                if (meshvertsptr != null)
                {
                    Vec3PointerAndOffsetToArray(ref meshVertsArr, meshvertsptr, meshVertCount);   
                }

                Vector3* meshNormalsptr = pointer->MMeshes[meshcount]->MNormals;
                Vector3[] meshNormalsArr = new Vector3[meshVertCount];
                if (meshNormalsptr != null)
                {
                    Vec3PointerAndOffsetToArray(ref meshNormalsArr, meshNormalsptr, meshVertCount);   
                }

                Vector3* meshTangentsptr = pointer->MMeshes[meshcount]->MTangents;
                Vector3[] meshTangentsArr = new Vector3[meshVertCount];
                if (meshTangentsptr != null)
                {
                    Vec3PointerAndOffsetToArray(ref meshTangentsArr, meshTangentsptr, meshVertCount);   
                }

                Vector3* meshBiTangentsptr = pointer->MMeshes[meshcount]->MTangents;
                Vector3[] meshBiTangentsArr = new Vector3[meshVertCount];
                if (meshBiTangentsptr != null)
                {
                    Vec3PointerAndOffsetToArray(ref meshBiTangentsArr, meshBiTangentsptr, meshVertCount);
                }

                meshes[meshcount] = new MeshData(meshVertsArr, meshUvsArr);

            }

            return meshes;
        }
        
        
        private static unsafe void Vec3PointerAndOffsetToArray(ref Vector3[] destination, Vector3* item, uint count )
        {
            if (destination == null)
            {
                throw new NullReferenceException("Destination is null!");
            }
            for (int i = 0; i < count; i++)
            {
                destination[i] = item[i];
            }
        }
    }
    
}