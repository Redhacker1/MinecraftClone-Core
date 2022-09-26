using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace Engine.Utilities.Concurrency;

public sealed class ThreadSafeList<T> : IList<T>
{
    private readonly List<T> _list = new List<T>();
    readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();


    public IEnumerator<T> GetEnumerator()
    {
        List<T> localList;

        _readerWriterLock.EnterReadLock();
        try
        {
            localList = new List<T>(_list);
        }
        finally { _readerWriterLock.ExitReadLock(); }

        foreach (T item in localList) yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }


    public ImmutableArray<T> ToImmutableArray()
    {
        ImmutableArray<T>.Builder test = ImmutableArray.CreateBuilder<T>();
        _readerWriterLock.EnterReadLock();
        test.Count = _list.Count;
        try
        {
            for (int i = 0; i < _list.Count; i++)
            {
                test[i] = _list[i];
            }
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return test.ToImmutable();

    }

    public void Add(T item)
    {
        _readerWriterLock.EnterWriteLock();
        try { _list.Add(item); }
        finally { _readerWriterLock.ExitWriteLock(); }
    }

    public void Clear()
    {
        _readerWriterLock.EnterWriteLock();
        try
        {
            _list.Clear();
        }
        finally{_readerWriterLock.ExitWriteLock();}
    }

    public bool Contains(T item)
    {
        _readerWriterLock.EnterReadLock();
        bool contains;
        try
        {
            contains = _list.Contains(item);
        }
        finally{_readerWriterLock.ExitReadLock();}

        return contains;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _readerWriterLock.EnterReadLock();
        try
        {
            _list.CopyTo(array, arrayIndex);
        }
        finally{_readerWriterLock.ExitReadLock();}
    }

    public bool Remove(T item)
    {
        _readerWriterLock.EnterWriteLock();
        bool remove;
        try
        {
            remove = _list.Remove(item);
        }
        finally{_readerWriterLock.ExitWriteLock();}
        return remove;
    }

    public int Count
    {
        get
        {
            int listCapacity;    
            _readerWriterLock.EnterReadLock();
            try
            {
                listCapacity = _list.Count;
            }
            finally{_readerWriterLock.ExitReadLock();}

            return listCapacity;
        }
    }
    public bool IsReadOnly => false;

    public int Capacity
    {
        get
        {
            int listCapacity;    
            _readerWriterLock.EnterReadLock();
            try
            {
                listCapacity = _list.Capacity;
            }
            finally{_readerWriterLock.ExitReadLock();}

            return listCapacity;
        }
    }

    public int IndexOf(T item)
    {
        _readerWriterLock.EnterReadLock();
        int index;  
        try
        {
            index = _list.IndexOf(item);
        }
        finally{_readerWriterLock.ExitReadLock();}

        return index;
    }

    public void Insert(int index, T item)
    {
        _readerWriterLock.EnterWriteLock();
        try
        {
            _list.Insert(index,item);
        }
        finally{_readerWriterLock.ExitWriteLock();}
    }

    public void RemoveAt(int index)
    {
        _readerWriterLock.EnterWriteLock();
        try
        {
            _list.RemoveAt(index);
        }
        finally{_readerWriterLock.ExitWriteLock();}
    }

    public T this[int index]
    {
        get
        {
            T returnValue;
            _readerWriterLock.EnterReadLock();
            try
            {
                returnValue = _list[index];
            }
            finally{_readerWriterLock.ExitReadLock();}

            return returnValue;
        }
        set
        {
            _readerWriterLock.EnterWriteLock();
            try
            {
                _list[index] = value;
            }
            finally{_readerWriterLock.ExitWriteLock();}
        }
    }

    public void TrimExcess()
    {
        _readerWriterLock.EnterWriteLock();
        try
        {
            _list.TrimExcess();
        }
        finally{_readerWriterLock.ExitWriteLock();}
    }
}