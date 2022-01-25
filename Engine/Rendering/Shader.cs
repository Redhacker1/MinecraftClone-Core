using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using SpirvReflectSharp;
using Veldrid;
using Veldrid.SPIRV;

namespace Engine.Rendering
{
    public class Shader : IDisposable
    {
        unsafe byte[] GetBytes(byte* ptr)
        {
      
            int length = 0;
            while (length < 16384 && ptr[length] != 0)
            {
                length++;   
            }
            // Store bytes in managed array.
            byte[] bytes = new byte[length];
            Marshal.Copy((IntPtr)ptr, bytes, 0, length);
            return bytes;
        }
        byte[] GetBytes(string stringdata)
        {
            return Encoding.UTF8.GetBytes(stringdata);
        }

        string GetString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes).Trim();
        }
        
        ShaderStages shaderstage;
        internal Veldrid.Shader shader;
        byte[] _spirvSrcCode;
        internal SpirvReflection ShaderData;

        public Shader(string path, GraphicsDevice device, ShaderStages stage)
        {
            shaderstage = stage;
            if (Path.GetExtension(path) == ".spv")
            {
                LoadShader(File.ReadAllBytes(path), stage, device);
                return;
            }
            else
            {
                LoadShaderGLSL(stage, device, path);
                return;
            }
            
        }

        public void Dispose()
        {
            shader.Dispose();
        }
        
        
        internal static bool HasSpirvHeader(byte[] bytes)
        {
            return bytes.Length > 4
                   && bytes[0] == 0x03
                   && bytes[1] == 0x02
                   && bytes[2] == 0x23
                   && bytes[3] == 0x07;
        }

        unsafe void LoadShader(byte[] bytecode, ShaderStages type, GraphicsDevice device, string Entrypoint = "main" )
        {
            _spirvSrcCode = bytecode; ;

            using (ShaderModule module = SpirvReflect.ReflectCreateShaderModule(bytecode))
            {
                module.GetCode();


                var out_vars = module.EnumerateOutputVariables();
                var push_constants = module.EnumeratePushConstants();
                Console.WriteLine("push_constants:\n");
                foreach (var constants in push_constants)
                {
                    Console.WriteLine(constants.TypeDescription.StorageClass);
                    foreach (var member in constants.Members)
                    {
                        Console.WriteLine(member.Name);
                        Console.WriteLine(member.TypeDescription.TypeFlags);
                    }
                    Console.WriteLine(constants.Name);
                    Console.WriteLine(constants.DecorationFlags);
                    Console.WriteLine(constants.Offset);
                    Console.WriteLine(constants.DecorationFlags);
                    Console.WriteLine(constants.TypeDescription.Op);
                    Console.WriteLine();
                }
            }


            byte[] result = CompileShader(device.BackendType, bytecode, type);

            ShaderDescription shaderDescription = new ShaderDescription
            {
                Stage = type,
                EntryPoint = Entrypoint,
                ShaderBytes = result
            };


            try
            {
                shader = device.ResourceFactory.CreateShader(shaderDescription);
            }
            catch (VeldridException e)
            {
                Console.WriteLine(GetString(result));
                throw;
            }
        }



        /// <summary>
        /// Compiles GLSL Shader code to SPIRV, makes a Veldrid Shader object and does some basic
        /// reflection to get the necessary pipeline data set up.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="device"></param>
        /// <param name="path"></param>
        void LoadShaderGLSL(ShaderStages type, GraphicsDevice device, string path, string Entrypoint = "main")
        {
            string src = File.ReadAllText(path);
            byte[] bytecode = GLSLToSPIRV(src, type);
            LoadShader(bytecode, type, device, Entrypoint);
        }


        byte[] GLSLToSPIRV(string text, ShaderStages stages)
        {
            SpirvCompilationResult result = SpirvCompilation.CompileGlslToSpirv(text, string.Empty, stages, GlslCompileOptions.Default);

            return result.SpirvBytes;

        }

        byte[] CompileShader(GraphicsBackend backend, byte[] bytecode, ShaderStages stage)
        {

            if (backend == GraphicsBackend.Metal)
            {
                if (stage == ShaderStages.Vertex)
                {
                    var data = SpirvCompilation.CompileVertexFragment(bytecode, null, CrossCompileTarget.MSL);
                    return GetBytes(data.FragmentShader);
                }

                if(stage == ShaderStages.Fragment)
                {
                    var data = SpirvCompilation.CompileVertexFragment(null, bytecode, CrossCompileTarget.MSL);
                    return GetBytes(data.FragmentShader);
                }
            }
            else if (backend == GraphicsBackend.OpenGL)
            {
                if (stage == ShaderStages.Vertex)
                {
                    var data = SpirvCompilation.CompileVertexFragment(bytecode, bytecode, CrossCompileTarget.GLSL);
                    return GetBytes(data.FragmentShader);
                }

                if(stage == ShaderStages.Fragment)
                {
                    var data = SpirvCompilation.CompileVertexFragment(bytecode, bytecode, CrossCompileTarget.GLSL);
                    return GetBytes(data.FragmentShader);
                }
            }
            else if (backend == GraphicsBackend.Direct3D11)
            {
                if (stage == ShaderStages.Vertex)
                {
                    var data = SpirvCompilation.CompileVertexFragment(bytecode, null, CrossCompileTarget.HLSL);
                    
                    return GetBytes(data.FragmentShader);
                }

                if(stage == ShaderStages.Fragment)
                {
                    var data = SpirvCompilation.CompileVertexFragment(null, bytecode, CrossCompileTarget.HLSL);
                    return GetBytes(data.FragmentShader);
                }
            }
            else if (backend == GraphicsBackend.Vulkan)
            {
                return bytecode;
            }
            else
            {
                throw new NotImplementedException("Use a more robust implementation than this hack");
            }

            return bytecode;
        }
        

    }
}
