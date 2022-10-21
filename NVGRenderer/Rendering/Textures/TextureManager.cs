namespace NVGRenderer.Rendering.Textures
{
    public sealed class TextureManager : IDisposable
    {
        readonly List<TextureSlot> _textures;
        readonly List<int> _availableIndexes = new List<int>();

        private int _count;

        private readonly TextureSlot _default = new TextureSlot();

        public TextureManager(NvgRenderer renderer)
        {

            _textures = new List<TextureSlot>();
            _count = 0;
        }

        public int AddTexture(TextureSlot slot)
        {
            int id = 0;

            if (_availableIndexes.Count > 0)
            {
                id = _availableIndexes[^1];
                _textures[id] = slot;
                _availableIndexes.RemoveAt(_availableIndexes.Count - 1);
                slot.Id = id;
            }
            else
            {
                id = _count++;
                slot.Id = id;
                _textures.Add(slot);
            }

            return id;

        }

        public TextureSlot FindTexture(int id, out bool success)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_textures[i].Id == id)
                {
                    success = true;
                    return _textures[i];
                }
            }

            success = false;
            return _default;
        }

        public bool DeleteTexture(int id)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_textures[i].Id == id)
                {
                    _textures[i].Dispose();
                    _textures[i] = default;
                    _availableIndexes.Add(i);
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            for (int i = 0; i < _count; i++)
            {
                _textures[i].Dispose();
            }
        }

    }
}