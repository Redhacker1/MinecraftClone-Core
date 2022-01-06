using System;
using Engine.AssetLoading;
using Engine.Rendering.Shared.Shaders;
using Veldrid;
using Shader = Engine.Rendering.Shared.Shaders.Shader;

namespace Engine.Rendering.VeldridBackend
{
    class VeldridShader : Shader
    {
        internal Veldrid.Shader BackingShader;
        ShaderStages veldridStage;
        ResourceFactory gpuFactory;
        VeldridShader(ShaderFormat _shaderFormat, string ShaderPath, ShaderSettings shaderSettings, ResourceFactory factory) : base(_shaderFormat, shaderSettings ,ShaderPath)
        {
            veldridStage = shaderSettings.ShaderStage switch
            {
                ShaderType.Compute => ShaderStages.Compute,
                ShaderType.Fragment => ShaderStages.Fragment,
                ShaderType.Geometry => ShaderStages.Geometry,
                ShaderType.None => ShaderStages.None,
                ShaderType.TesselationControl => ShaderStages.TessellationControl,
                ShaderType.TesselationEvaluation => ShaderStages.TessellationEvaluation,
                ShaderType.Vertex => ShaderStages.Vertex,
                _ => throw new ArgumentOutOfRangeException(nameof(shaderSettings.ShaderStage), shaderSettings.ShaderStage, null)
            };
            gpuFactory = factory;
            
        }

        internal override void Compile()
        {
            // TODO: Add ability to transpile to HLSL, GLSL, SPIRV
            var shadercode = ShaderLoader.LoadShaderSpirv(ShaderPath, _shaderType, shaderFormat);
            ShaderDescription shader = new ShaderDescription()
            {
                Debug = false,
                EntryPoint = "main",
                Stage = veldridStage,
                ShaderBytes = shadercode.SpirvBytes
            };

            BackingShader = gpuFactory.CreateShader(shader);
        }
    }
}