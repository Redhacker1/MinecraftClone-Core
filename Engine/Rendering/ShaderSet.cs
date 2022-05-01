namespace Engine.Rendering
{
    public class ShaderSet
    {
        Veldrid.Shader vsShader;
        Veldrid.Shader fsShader;
        ShaderSet(Shader vs, Shader fs)
        {
            vsShader = vs.shader;
            fsShader = fs.shader;
        }
    }
}