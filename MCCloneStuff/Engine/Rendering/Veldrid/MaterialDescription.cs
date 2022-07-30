using System.Collections.Generic;
using Veldrid;

namespace Engine.Rendering.Veldrid
{
    public struct MaterialDescription
    {
        public bool WriteDepthBuffer;
        public bool DepthTest;

        public ComparisonKind ComparisonKind;
        public FaceCullMode CullMode;
        public PolygonFillMode FillMode;
        public FrontFace FaceDir;
        public PrimitiveTopology Topology;
        public IReadOnlyDictionary<ShaderStages, Shader> Shaders;
        public BlendStateDescription BlendState;
        
        

        public MaterialDescription(bool WriteDepth, bool DepthTest, ComparisonKind kind, FaceCullMode cullMode, PolygonFillMode filltype, FrontFace facedir,PrimitiveTopology topology, IReadOnlyDictionary<ShaderStages, Shader> shaders, BlendStateDescription blendState)
        {

            WriteDepthBuffer = WriteDepth;
            this.DepthTest = DepthTest;
            
            
            ComparisonKind = kind;
            CullMode = cullMode;
            FillMode = filltype;
            FaceDir = facedir;
            Shaders = shaders;
            BlendState = blendState;
            Topology = topology;
        }
        
        
        
    }
}