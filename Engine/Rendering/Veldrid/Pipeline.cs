using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Engine.Rendering.Abstract;
using Veldrid;
using Veldrid.SPIRV;
using Shader = Veldrid.Shader;

namespace Engine.Rendering.Veldrid
{
    public struct Pipeline : IEquatable<Pipeline>
    {
        ushort PipelineMask;
        readonly bool WriteDepthBuffer;
        readonly bool DepthTest;
        readonly ComparisonKind ComparisonKind;
        readonly BlendStateDescription blendState;
        readonly FaceCullMode _cullMode;
        readonly PolygonFillMode FillMode;
        readonly FrontFace faceDir;
        readonly PrimitiveTopology _topology;
        readonly ShaderSet _shaders;

        internal global::Veldrid.Pipeline _pipeline;

        public Pipeline(bool depthTest, bool writeDepthBuffer, ComparisonKind compare, FaceCullMode cullmode, FrontFace dir, PrimitiveTopology topology, PolygonFillMode filltype, ShaderSet shaders, GraphicsDevice device, VertexLayoutDescription[] vertexLayout, ResourceLayout[] layouts)
        {
            WriteDepthBuffer = writeDepthBuffer;
            DepthTest = depthTest;
            ComparisonKind = compare;
            blendState = BlendStateDescription.SingleOverrideBlend;
            _cullMode = cullmode;
            faceDir = dir;
            _topology = topology;
            FillMode = filltype;
            _shaders = shaders;


            List<Shader> Shader = new List<Shader>();

            if (device.BackendType != GraphicsBackend.Vulkan)
            {
                CrossCompileTarget backend = device.BackendType switch
                {
                    GraphicsBackend.Metal => CrossCompileTarget.MSL,
                    GraphicsBackend.Direct3D11 => CrossCompileTarget.HLSL,
                    GraphicsBackend.OpenGL => CrossCompileTarget.GLSL,
                    GraphicsBackend.OpenGLES => CrossCompileTarget.ESSL,
                    GraphicsBackend.Vulkan => throw new InvalidOperationException(), // This should never happen, but it makes rider happy...
                    _ => throw new ArgumentOutOfRangeException()

                };


                var vertexByteCode = _shaders.Vertex.Shaderbytes.ToArray();
                var fragmentByteCode = _shaders.Fragment.Shaderbytes.ToArray();
                
                
                VertexFragmentCompilationResult output = SpirvCompilation.CompileVertexFragment(
                    vertexByteCode,
                    fragmentByteCode,
                    backend, new CrossCompileOptions());
                
                Shader.Add(device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment, Encoding.Default.GetBytes(output.FragmentShader), shaders.Fragment.EntryPoint)));
                Shader.Add(device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex, Encoding.Default.GetBytes(output.VertexShader), shaders.Vertex.EntryPoint)));
            }
            else
            {
                Shader.Add(device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment, shaders.Fragment.Shaderbytes.ToArray(), shaders.Fragment.EntryPoint)));
                Shader.Add(device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex, shaders.Vertex.Shaderbytes.ToArray(), shaders.Vertex.EntryPoint)));
            }
            
                
            ShaderSetDescription shaderSet = new ShaderSetDescription
            {
                VertexLayouts = vertexLayout,
                Shaders = Shader.ToArray(),
            };


            ushort pipeline = (ushort)(BoolToNum(WriteDepthBuffer) << 15); // boolean, 1 bit
            pipeline |= (ushort)(BoolToNum(DepthTest) << 14); // boolean, 1 bit
            pipeline |= (ushort)((ushort)compare << 13); // enum range 0-7, three bits 
            pipeline |= (ushort)((ushort)cullmode << 10); // enum range 0-2, two bits 
            pipeline |= (ushort)((ushort)dir << 8); // enum range 0-1, one bit
            pipeline |= (ushort)((ushort)topology << 7); // enum range 0-4, 3 bit
            pipeline |= (ushort)((ushort)filltype << 3); // enum range 0-1, 1 bit
            
            
            PipelineMask = pipeline;

            GraphicsPipelineDescription description =
                new GraphicsPipelineDescription
                (
                    blendState,
                    new DepthStencilStateDescription(depthTest, writeDepthBuffer, compare),
                    new RasterizerStateDescription(cullmode, filltype, faceDir, true, false),
                    topology, 
                    shaderSet, 
                    layouts, 
                    device.MainSwapchain.Framebuffer.OutputDescription
                    );

            _pipeline = device.ResourceFactory.CreateGraphicsPipeline(description);
            Console.WriteLine("Pipeline completed!");
        }

        // WIP: This method should not currently be trusted to actually calculate equality, however if the Pipeline is equal, this will always return true!
        // Likely to be used for rendering and optimization passes.
        public bool Equals(Pipeline other)
        {
            // TODO: Ensure that the shaders and (eventually) resources are the same before saying it is true
            if (PipelineMask == other.PipelineMask)
            {
                //TODO: Make sure the internals of this are actually reliable!
                if (other.blendState.Equals(blendState))
                {
                    if (_shaders.Equals(other._shaders))
                    {
                        return true;     
                    }
                }
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            return obj is Pipeline other && Equals(other);
        }

        /// <summary>
        /// This hash code will reflect most of the pipeline being equal to this object, the exception notable exceptions being the blendstate and the shader configurations
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            ushort pipeline = (ushort)(BoolToNum(WriteDepthBuffer) << 15); // boolean, 1 bit
            pipeline |= (ushort)(BoolToNum(DepthTest) << 14); // boolean, 1 bit
            pipeline |= (ushort)((ushort)ComparisonKind << 13); // enum range 0-7, three bits 
            pipeline |= (ushort)((ushort)_cullMode << 10); // enum range 0-2, two bits 
            pipeline |= (ushort)((ushort)faceDir << 8); // enum range 0-1, one bit
            pipeline |= (ushort)((ushort)_topology << 7); // enum range 0-4, 3 bit
            pipeline |= (ushort)((ushort)FillMode << 3); // enum range 0-1, 1 bit
            return pipeline;
        }

        public static bool operator ==(Pipeline left, Pipeline right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Pipeline left, Pipeline right)
        {
            return !(left == right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        static ushort BoolToNum(bool booleanValue)
        {
            return (ushort)(booleanValue ? 1 : 0);
        }
    }

}