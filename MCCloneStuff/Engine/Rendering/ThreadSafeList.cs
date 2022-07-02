using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Engine.Rendering
{
    public class ThreadSafeList<T> : IList<T>
    {
        List<T> backingList = new List<T>();
        ReaderWriterLockSlim internalLock = new ReaderWriterLockSlim();
        public IEnumerator<T> GetEnumerator()
        {
            return backingList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            if (internalLock.IsWriteLockHeld == false)
            {
                internalLock.EnterWriteLock();
            }
            backingList.Add(item);
            internalLock.ExitWriteLock();

        }

        public void Clear()
        {
            if (internalLock.IsWriteLockHeld == false)
            {
                internalLock.EnterWriteLock();
            }
            backingList.Clear();
            internalLock.ExitWriteLock();
        }

        public bool Contains(T item)
        {
            if (internalLock.IsReadLockHeld == false)
            {
                internalLock.EnterReadLock();
            }
            bool result = backingList.Contains(item);
            internalLock.ExitReadLock();
            return result;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (internalLock.IsReadLockHeld == false)
            {
                internalLock.EnterReadLock();
            }
            backingList.CopyTo(array, arrayIndex);
            internalLock.ExitReadLock();
        }

        public bool Remove(T item)
        {
            if (internalLock.IsWriteLockHeld == false)
            {
                internalLock.EnterWriteLock();
            }

            bool result = backingList.Remove(item);
            internalLock.ExitWriteLock();
            return result;
        }

        public int Count => backingList.Count;
        public bool IsReadOnly => false;
        
        public int IndexOf(T item)
        {
            if (internalLock.IsReadLockHeld == false)
            {
                internalLock.EnterReadLock();
            }
            int result = backingList.IndexOf(item);
            internalLock.ExitReadLock();
            return result;
        }

        public void Insert(int index, T item)
        {
            if (internalLock.IsWriteLockHeld == false)
            {
                internalLock.EnterWriteLock();
            }
            backingList.Insert(index, item);
            internalLock.ExitWriteLock();
        }

        public void RemoveAt(int index)
        {
            if (internalLock.IsWriteLockHeld == false)
            {
                internalLock.EnterWriteLock();
            }
            backingList.RemoveAt(index);
            internalLock.ExitWriteLock();
        }

        public T this[int index]
        {
            get
            {
                if (internalLock.IsReadLockHeld == false)
                {
                    internalLock.EnterReadLock();
                }
                T result = backingList[index];
                internalLock.ExitReadLock();
                return result;

            }
            set
            {
                if (internalLock.IsWriteLockHeld == false)
                {
                    internalLock.EnterWriteLock();
                }
                backingList.Insert(index, value);
                internalLock.ExitWriteLock();
            }
        }

        public void EnterReadLock()
        {
            if (internalLock.IsReadLockHeld == false)
            {
                internalLock.EnterReadLock();
            }
        }

        public void ExitReadLock()
        {
            if (internalLock.IsReadLockHeld)
            {
                internalLock.ExitReadLock();   
            }
        }

        public void EnterWriteLock()
        {
            if (internalLock.IsWriteLockHeld == false)
            {
                internalLock.EnterWriteLock();   
            }
        }

        public void ExitWriteLock()
        {
            if (internalLock.IsWriteLockHeld)
            {
                internalLock.ExitWriteLock();   
            }
        }

        public void DoOnReadLock(Action ToDo)
        {
            internalLock.EnterReadLock();
            ToDo.Invoke();
            internalLock.ExitReadLock();
        }
        public void DoOnWriteLock(Action ToDo)
        {
            internalLock.EnterWriteLock();
            ToDo.Invoke();
            internalLock.ExitWriteLock();
        }
        
        
    }
}