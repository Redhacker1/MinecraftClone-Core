using System;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Spirzza.Interop.Shaderc;

namespace Engine.Rendering.Abstract;

public static class ShaderExtensions
{
    public enum ShadingLanguage : byte
    {
        GLSL,
        HLSL,
        SPIRV
    }
    
    public static Shader CreateShaderSPIRV(ShaderType type, byte[] bytes, string EntryPoint )
    {
        return new Shader(ImmutableArray.Create(bytes), type, EntryPoint);
    }
    
    public static Shader CreateShaderFromFile(ShaderType type, string path, string entryPoint, ShadingLanguage shadingLanguage )
    {
        byte[] fileBytes = File.ReadAllBytes(path);
        return CreateShaderFromBytes(type, fileBytes, path, entryPoint, shadingLanguage);
    }
    
    public static unsafe Shader CreateShaderFromBytes(ShaderType type, ReadOnlySpan<byte> shaderBytes, string name, string entryPoint, ShadingLanguage shadingLanguage  )
    {
        Shader shaderObject;
        if (shadingLanguage != ShadingLanguage.SPIRV)
        {
            
            Span<sbyte> pathTest = MemoryMarshal.Cast<byte, sbyte>(Encoding.UTF8.GetBytes(name));
            Span<sbyte> entryTest = MemoryMarshal.Cast<byte, sbyte>(Encoding.UTF8.GetBytes(entryPoint));
            ReadOnlySpan<sbyte> bytes = MemoryMarshal.Cast<byte, sbyte>(shaderBytes);
            
            shaderc_shader_kind shaderKind = type switch
            {
                ShaderType.Fragment => shaderc_shader_kind.shaderc_fragment_shader,
                ShaderType.Vertex => shaderc_shader_kind.shaderc_vertex_shader,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            };
            
            shaderc_source_language lang = shadingLanguage switch
            {
                ShadingLanguage.GLSL => shaderc_source_language.shaderc_source_language_glsl,
                ShadingLanguage.HLSL => shaderc_source_language.shaderc_source_language_hlsl,
                _ => throw new ArgumentOutOfRangeException(nameof(shadingLanguage), shadingLanguage, null)
            };
            
            shaderc_compiler* compiler = Shaderc.shaderc_compiler_initialize();

            shaderc_compile_options* options = Shaderc.shaderc_compile_options_initialize();
            Shaderc.shaderc_compile_options_set_source_language(options, lang);
            Shaderc.shaderc_compile_options_set_optimization_level(options, shaderc_optimization_level.shaderc_optimization_level_zero);
            Shaderc.shaderc_compile_options_set_target_env(options, shaderc_target_env.shaderc_target_env_vulkan, 1);
#if DEBUG
            Shaderc.shaderc_compile_options_set_generate_debug_info(options);
#endif
            shaderc_compilation_result* result;
            fixed (sbyte* pathPtr = pathTest)
            fixed (sbyte* filePtr = bytes)
            fixed (sbyte* entrypointPtr = entryTest)
            {
                result = Shaderc.shaderc_compile_into_spv(compiler,filePtr, (nuint)bytes.Length, shaderKind, pathPtr, entrypointPtr, options);
            }
            shaderc_compilation_status status = Shaderc.shaderc_result_get_compilation_status(result);
            nuint errorCount = Shaderc.shaderc_result_get_num_errors(result);
            nuint warningCount = Shaderc.shaderc_result_get_num_warnings(result);
            sbyte* errormessage = Shaderc.shaderc_result_get_error_message(result);
            sbyte* shaderCode = Shaderc.shaderc_result_get_bytes(result);
            
            string errors = Marshal.PtrToStringAnsi((IntPtr) errormessage);
            Console.WriteLine($"Compiled with {warningCount} Warnings:\n{errors}");
            
            if (status != shaderc_compilation_status.shaderc_compilation_status_success || errorCount > 0)
            {
                Console.WriteLine($"Compiled with {errorCount} Errors,\n{errors}");
            
                throw new Exception($"Error compiling shader {errors}");
            }
            
            nuint length = Shaderc.shaderc_result_get_length(result);
            if (length > int.MaxValue)
            {
                throw new Exception("What the hell kind of shader are you using?, it is over 2gb in size!");
            }
            Span<byte> code = new Span<byte>(shaderCode, (int) length);
            shaderObject = new Shader(code.ToArray().ToImmutableArray(), type, entryPoint);
            
            Shaderc.shaderc_compiler_release(compiler);
            Shaderc.shaderc_result_release(result);
            Shaderc.shaderc_compile_options_release(options);
        }
        else
        {
            shaderObject = new Shader(shaderBytes.ToArray().ToImmutableArray(), type, entryPoint);
        }
        return shaderObject;
    }


}