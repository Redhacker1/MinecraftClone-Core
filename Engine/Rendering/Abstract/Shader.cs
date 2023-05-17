using System.Collections.Immutable;

namespace Engine.Rendering.Abstract
{

    /// <summary>
    /// SPIR-V compiled Shader
    /// </summary>
    public struct Shader
    {
        /// <summary>
        /// SPIR-V Shader
        /// </summary>
        public readonly ImmutableArray<byte> Shaderbytes;
        public readonly ShaderType Stage;
        public readonly string EntryPoint;

        public Shader(ImmutableArray<byte> shaderbytes, ShaderType stage, string entryPoint)
        {
            Stage = stage;
            Shaderbytes = shaderbytes;
            EntryPoint = entryPoint;
        }
        
    }
}
