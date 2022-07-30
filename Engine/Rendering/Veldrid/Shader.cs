using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace Engine.Rendering.Veldrid
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
        internal global::Veldrid.Shader shader;
        Memory<byte> _spirvSrcCode;
        internal SpirvReflection ShaderData;

        public Shader(string path, GraphicsDevice device, ShaderStages stage)
        {
            shaderstage = stage;
            if (Path.GetExtension(path) == ".spv")
            {
                LoadShader(File.ReadAllBytes(path), stage, device);
                return;
            }

            LoadShaderGLSL(stage, device, path);
        }

        public Shader()
        {
            
        }

        public static Shader LoadShaderText(string text, GraphicsDevice device, ShaderStages stage)
        {
            Shader shader = new Shader();
            shader.LoadShader(Encoding.UTF8.GetBytes(text), stage, device);
            return shader;
        }
        public static Shader LoadShaderBytes(byte[] bytes, GraphicsDevice device, ShaderStages stage)
        {
            Shader shader = new Shader();
            shader.LoadShader(bytes, stage, device);
            return shader;
        }
        

        public void Dispose()
        {
            shader.Dispose();
        }
        
        
        internal static bool HasSpirvHeader(Memory<byte> bytes)
        {
            var bytedata = bytes.Span;
            return bytes.Length > 4
                   && bytedata[0] == 0x03
                   && bytedata[1] == 0x02
                   && bytedata[2] == 0x23
                   && bytedata[3] == 0x07;
        }

        void LoadShader(Memory<byte> bytecode, ShaderStages type, GraphicsDevice device, string Entrypoint = "main" )
        {
            _spirvSrcCode = bytecode;


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
            catch (VeldridException)
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

        byte[] CompileShader(GraphicsBackend backend, Memory<byte> bytecode, ShaderStages stage)
        {

            if (backend == GraphicsBackend.Metal)
            {
                if (stage == ShaderStages.Vertex)
                {
                    var data = SpirvCompilation.CompileVertexFragment(bytecode.ToArray(), null, CrossCompileTarget.MSL);
                    return GetBytes(data.FragmentShader);
                }

                if(stage == ShaderStages.Fragment)
                {
                    var data = SpirvCompilation.CompileVertexFragment(null, bytecode.ToArray(), CrossCompileTarget.MSL);
                    return GetBytes(data.FragmentShader);
                }
            }
            else if (backend == GraphicsBackend.OpenGL)
            {
                if (stage == ShaderStages.Vertex)
                {
                    var data = SpirvCompilation.CompileVertexFragment(bytecode.ToArray(), bytecode.ToArray(), CrossCompileTarget.GLSL);
                    return GetBytes(data.FragmentShader);
                }

                if(stage == ShaderStages.Fragment)
                {
                    var data = SpirvCompilation.CompileVertexFragment(bytecode.ToArray(), bytecode.ToArray(), CrossCompileTarget.GLSL);
                    return GetBytes(data.FragmentShader);
                }
            }
            else if (backend == GraphicsBackend.Direct3D11)
            {
                if (stage == ShaderStages.Vertex)
                {
                    var data = SpirvCompilation.CompileVertexFragment(bytecode.ToArray(), bytecode.ToArray(), CrossCompileTarget.HLSL);
                    
                    return GetBytes(data.FragmentShader);
                }

                if(stage == ShaderStages.Fragment)
                {
                    var data = SpirvCompilation.CompileVertexFragment(bytecode.ToArray(), bytecode.ToArray(), CrossCompileTarget.HLSL);
                    return GetBytes(data.FragmentShader);
                }
            }
            else if (backend == GraphicsBackend.Vulkan && HasSpirvHeader(bytecode))
            {
                return bytecode.ToArray();
            }
            else
            {
                throw new NotImplementedException("Use a more robust implementation than this hack");
            }

            return bytecode.ToArray();
        }
        

    }
}
