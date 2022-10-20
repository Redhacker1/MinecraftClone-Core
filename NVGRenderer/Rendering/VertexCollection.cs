using Engine.Utilities.LowLevel.Memory;
using SilkyNvg.Rendering;

namespace NVGRenderer.Rendering
{
    class VertexCollection
    {
        readonly ByteStore<Vertex> _vertices;
        int _count;

        public int CurrentsOffset => _count;

        public ReadOnlySpan<Vertex> Vertices => _vertices.Span;

        public VertexCollection()
        {
            _vertices = ByteStore<Vertex>.Create(NativeMemoryHeap.Instance, 10);
            _count = 0;
        }

        private void AllocVerts(int n)
        {
            if (_count + n > (int) _vertices.Capacity)
            {
                _vertices.PrepareCapacityFor((uint)n);
            }
        }

        public void AddVertex(Vertex vertex)
        {
            AllocVerts(1);
            _vertices.Append(vertex);
            _count++;
        }

        public void AddVertices(ICollection<Vertex> vertices)
        {
            AllocVerts(vertices.Count);

            _vertices.AppendRange(vertices.ToArray());
            _count += vertices.Count;
        }

        public void Clear()
        {
            _vertices.Clear();
            _count = 0;
        }

        ~VertexCollection()
        {
            _vertices.Dispose();
        }

    }
}