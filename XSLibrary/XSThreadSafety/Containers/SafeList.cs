using System.Collections.Generic;
using XSLibrary.ThreadSafety.Executors;

namespace XSLibrary.ThreadSafety.Containers
{
    /// <summary>
    /// List which is protected from multiple accesses. From wherever the list is accessed it is in a stable state regarding its elements.
    /// The list does not protect the elements themselves but the structure of the list.
    /// </summary>
    public class SafeList<T> // : IList<T>
    {
        List<T> m_internalList;
        SafeReadWriteExecutor m_safeExecutor;

        public SafeList() : this(new RWExecutorUnlimited()) { }

        /// <param name="safeExecutor">Use your own implementation of an executor.</param>
        public SafeList(SafeReadWriteExecutor safeExecutor)
        {
            m_internalList = new List<T>();
            m_safeExecutor = safeExecutor;
        }

        /// <summary>
        /// Returns a snapshot-copy of the list. Accessing the elements is NOT THREADSAFE.
        /// </summary>
        public T[] Entries { get { return m_safeExecutor.ExecuteReadonly(() => m_internalList.ToArray()); } }

        public void Add(T item)
        {
            m_safeExecutor.Execute(() => m_internalList.Add(item));
        }

        public void Insert(int index, T element)
        {
            m_safeExecutor.Execute(() => m_internalList.Insert(index, element));
        }

        public bool Remove(T item)
        {
            return m_safeExecutor.Execute(() => m_internalList.Remove(item));
        }

        public void RemoveAt(int index)
        {
            m_safeExecutor.Execute(() => m_internalList.RemoveAt(index));
        }

        public T this[int index]
        {
            get { return GetElement(index); }
            set { SetElement(index, value); }
        }

        public T GetElement(int index)
        {
            return m_safeExecutor.ExecuteReadonly(() => m_internalList[index]);
        }

        public void SetElement(int index, T element)
        {
            m_safeExecutor.Execute(() => m_internalList[index] = element);
        }

        public int Count { get { return m_safeExecutor.ExecuteReadonly(() => m_internalList.Count); } }
    }
}