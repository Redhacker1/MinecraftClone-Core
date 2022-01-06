using System;
using System.IO;
using Engine.Rendering.Shared.Shaders;
using Veldrid;
using Veldrid.SPIRV;
using ShaderType = Engine.Rendering.Shared.Shaders.ShaderType;

namespace Engine.AssetLoading
{
    public class ShaderLoader
    {
        public static SpirvCompilationResult LoadShaderSpirv(string path, ShaderType stage, ShaderFormat type)
        {
            SpirvCompilationResult spirvCompilationResult = null;
            ShaderStages shaderStages = stage switch
            {
                ShaderType.Compute => ShaderStages.Compute,
                ShaderType.Fragment => ShaderStages.Fragment,
                ShaderType.Geometry => ShaderStages.Geometry,
                ShaderType.None => ShaderStages.None,
                ShaderType.TesselationControl => ShaderStages.TessellationControl,
                ShaderType.TesselationEvaluation => ShaderStages.TessellationEvaluation,
                ShaderType.Vertex => ShaderStages.Vertex,
                _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, null)
            };

            if (type == ShaderFormat.Glsl)
            {
                string stream = File.ReadAllText(path);

                if (stage == ShaderType.Vertex || stage == ShaderType.Fragment)
                {
                    spirvCompilationResult = 
                        SpirvCompilation.CompileGlslToSpirv(stream, null, shaderStages, GlslCompileOptions.Default);
                }
            }

            return spirvCompilationResult;
        }
    }
}