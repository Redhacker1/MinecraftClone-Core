using Engine.Rendering.Abstract;
using Veldrid;

namespace Engine.Rendering.VeldridBackend
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
        public ShaderSet Shaders;
        public BlendStateDescription BlendState;
        
        

        public MaterialDescription(bool WriteDepth, bool DepthTest, ComparisonKind kind, FaceCullMode cullMode, PolygonFillMode filltype, FrontFace facedir,PrimitiveTopology topology, ShaderSet shaders, BlendStateDescription blendState)
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