using System;
using System.Collections.Generic;

namespace MCClone_Core.World_CS.Generation;

public class FakeByteStore <T> where T : unmanaged
{
    List<T> backingList = new List<T>();


    public void PrepareCapacityFor(int count)
    {
        backingList.EnsureCapacity(count + backingList.Count);
    }

    public void AppendRange(Span<T> range)
    {
        backingList.AddRange(range.ToArray());
    }

    public Span<T> AsSpan()
    {
        return backingList.ToArray().AsSpan();
    }
}