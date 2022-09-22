namespace Engine.Rendering.Abstract;

/// <summary>
/// A pair of vertex and fragment SPIR-V shaders
/// </summary>
public struct ShaderSet
{

    public ShaderSet(Shader vertex, Shader fragment)
    {
        Fragment = fragment;
        Vertex = vertex;
    }
    public readonly Shader Fragment;
    public readonly Shader Vertex;
}