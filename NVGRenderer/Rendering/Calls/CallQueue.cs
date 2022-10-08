using System.Collections.Generic;
using Veldrid;

namespace NVGRenderer.Rendering.Calls
{
    internal class CallQueue
    {

        private readonly Queue<Call> _calls = new();

        public bool HasCalls => _calls.Count > 0;
        public int CallCount => _calls.Count;

        public void Add(Call call)
        {
            _calls.Enqueue(call);
        }

        public void Run(Frame frame, CommandList commandBuffer)
        {
            foreach (Call call in _calls)
            {
                call.Run(frame, commandBuffer);
            }
        }

        public void Clear()
        {
            _calls.Clear();
        }

    }
}