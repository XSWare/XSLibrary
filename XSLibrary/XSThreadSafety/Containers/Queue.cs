using System.Collections.Generic;
using XSLibrary.ThreadSafety.Executors;

namespace XSLibrary.ThreadSafety.Containers
{
    public class SafeQueue<T>
    {
        LinkedList<T> m_list;
        SafeReadWriteExecutor m_safeExecutor;

        public SafeQueue() : this(new RWExecutorUnlimited()) { }
        public SafeQueue(SafeReadWriteExecutor safeExecutor)
        {
            m_list = new LinkedList<T>();
            m_safeExecutor = safeExecutor;
        }

        public void Add(T item)
        {
            m_safeExecutor.Execute(() => m_list.AddLast(item));
        }

        public T Pop()
        {
            return m_safeExecutor.Execute(() => PopUnsafe());
        }

        private T PopUnsafe()
        {
            if (m_list.Count < 1)
                return default(T);

            T returnVal = m_list.First.Value;
            m_list.RemoveFirst();
            return returnVal;
        }

        public int Count { get { return m_safeExecutor.ExecuteReadonly(() => m_list.Count); } }

        public void Clear()
        {
            m_safeExecutor.Execute(() => m_list.Clear());
        }
    }
}
