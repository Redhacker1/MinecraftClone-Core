using System;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using SharpGen.Runtime;
using Spirzza.Interop.Shaderc;
using Vortice.D3DCompiler;

namespace Engine.Rendering.Abstract;

public static class ShaderExtensions
{
    public static Shader CreateShaderSpirv(ShaderType type, byte[] bytes, string EntryPoint )
    {
        return new Shader(ImmutableArray.Create(bytes), type, EntryPoint);
    }
    
    public static unsafe Shader CreateShaderHLSL(ShaderType type, string path, string entryPoint )
    {


        Span<sbyte> pathTest = MemoryMarshal.Cast<byte, sbyte>(Encoding.UTF8.GetBytes(path));
        Span<sbyte> entryTest = MemoryMarshal.Cast<byte, sbyte>(Encoding.UTF8.GetBytes(entryPoint));
        Span<sbyte> bytes = MemoryMarshal.Cast<byte, sbyte>(File.ReadAllBytes(path));
        

        shaderc_shader_kind shaderKind = type switch
        {
            ShaderType.Fragment => shaderc_shader_kind.shaderc_fragment_shader,
            ShaderType.Vertex => shaderc_shader_kind.shaderc_vertex_shader,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };

        shaderc_compiler* compiler = Shaderc.shaderc_compiler_initialize();

        shaderc_compile_options* options = Shaderc.shaderc_compile_options_initialize();
        Shaderc.shaderc_compile_options_set_source_language(options, shaderc_source_language.shaderc_source_language_hlsl);
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

        Shader shaderObject = new Shader(code.ToArray().ToImmutableArray(), type, entryPoint);
        
        Shaderc.shaderc_compiler_release(compiler);
        Shaderc.shaderc_result_release(result);
        Shaderc.shaderc_compile_options_release(options);
        
        return shaderObject;
        

    }
    
    public static unsafe Shader CreateShaderGLSL(ShaderType type, string path, string entryPoint )
    {


        Span<sbyte> pathTest = MemoryMarshal.Cast<byte, sbyte>(Encoding.UTF8.GetBytes(path));
        Span<sbyte> entryTest = MemoryMarshal.Cast<byte, sbyte>(Encoding.UTF8.GetBytes(entryPoint));
        Span<sbyte> bytes = MemoryMarshal.Cast<byte, sbyte>(File.ReadAllBytes(path));
        

        shaderc_shader_kind shaderKind = type switch
        {
            ShaderType.Fragment => shaderc_shader_kind.shaderc_fragment_shader,
            ShaderType.Vertex => shaderc_shader_kind.shaderc_vertex_shader,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };

        shaderc_compiler* compiler = Shaderc.shaderc_compiler_initialize();

        shaderc_compile_options* options = Shaderc.shaderc_compile_options_initialize();
        Shaderc.shaderc_compile_options_set_source_language(options, shaderc_source_language.shaderc_source_language_glsl);
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

        Shader shaderObject = new Shader(code.ToArray().ToImmutableArray(), type, entryPoint);
        
        Shaderc.shaderc_compiler_release(compiler);
        Shaderc.shaderc_result_release(result);
        Shaderc.shaderc_compile_options_release(options);
        
        return shaderObject;
        

    }
    
    
}