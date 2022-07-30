namespace Engine.Rendering.Abstract;

public enum BufferType : ushort
{
    Error = 1 << 0,
    Uniform = 1 << 1,
    Vertex = 1 << 2,
    Index = 1 << 3,
    Staging = 1 << 4
}