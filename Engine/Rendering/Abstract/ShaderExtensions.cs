using System;
using System.Collections.Immutable;
using shaderc;
using Result = shaderc.Result;

namespace Engine.Rendering.Abstract;

public static class ShaderExtensions
{
    public static Shader CreateShaderSpirv(ShaderType type, byte[] bytes, string EntryPoint )
    {
        return new Shader(ImmutableArray.Create(bytes), type, EntryPoint);
    }
    
    public static unsafe Shader CreateShaderHLSL(ShaderType type, string Path, string EntryPoint )
    {

        ShaderKind shaderKind = type switch
        {
            ShaderType.Fragment => ShaderKind.FragmentShader,
            ShaderType.Vertex => ShaderKind.VertexShader,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
        
        
        
        Compiler compiler = new Compiler();

        compiler.Options.SourceLanguage = SourceLanguage.Hlsl;
        compiler.Options.Optimization = OptimizationLevel.Zero;
        compiler.Options.SetTargetEnvironment(TargetEnvironment.Vulkan, EnvironmentVersion.Vulkan_1_0);

#if DEBUG
        compiler.Options.EnableDebugInfo();  
#endif
        
        Result test= compiler.Compile(Path, shaderKind, EntryPoint);
        

        if (test.Status != Status.Success || test.ErrorCount > 0)
        {
            Console.WriteLine($"Compiled with {test.ErrorCount} Errors, {test.WarningCount} Warnings: \n {test.ErrorMessage}");
            
            throw new Exception($"Error compiling shader {test.ErrorMessage}");
        }

        if (test.CodeLength > int.MaxValue)
        {
            throw new Exception("What the hell kind of shader are you using?, it is over 4gb in size!");
        }

        Span<byte> code = new Span<byte>((void*) test.CodePointer, (int) test.CodeLength);

        Shader shaderObject = new Shader(code.ToArray().ToImmutableArray(), type, EntryPoint);
        compiler.Dispose();
        return shaderObject;
        

    }
    
    public static unsafe Shader CreateShaderGLSL(ShaderType type, string Path, string EntryPoint )
    {

        ShaderKind shaderKind = type switch
        {
            ShaderType.Fragment => ShaderKind.FragmentShader,
            ShaderType.Vertex => ShaderKind.VertexShader,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),

        };

        Compiler compiler = new Compiler();

        compiler.Options.SourceLanguage = SourceLanguage.Glsl;
        compiler.Options.Optimization = OptimizationLevel.Zero;
        compiler.Options.SetTargetEnvironment(TargetEnvironment.Vulkan, EnvironmentVersion.Vulkan_1_0);

#if DEBUG
        compiler.Options.EnableDebugInfo();  
#endif
        
        Result test= compiler.Compile(Path, shaderKind, EntryPoint);
        

        if (test.Status != Status.Success || test.ErrorCount > 0)
        {
            Console.WriteLine($"Compiled with {test.ErrorCount} Errors, {test.WarningCount} Warnings: \n {test.ErrorMessage}");
            
            throw new Exception($"Error compiling shader {test.ErrorMessage}");
        }

        if (test.CodeLength > int.MaxValue)
        {
            throw new Exception("What the hell kind of shader are you using?, it is over 4gb in size!");
        }

        Span<byte> code = new Span<byte>((void*) test.CodePointer, (int) test.CodeLength);

        Shader shaderObject = new Shader(code.ToArray().ToImmutableArray(), type, EntryPoint);
        compiler.Dispose();
        return shaderObject;
    }
}