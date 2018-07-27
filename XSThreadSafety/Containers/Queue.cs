using System.Collections.Generic;
using XSLibrary.ThreadSafety.Executors;

namespace XSLibrary.ThreadSafety.Containers
{
    public class SafeQueue<T> : DataContainer<T>
    {
        LinkedList<T> m_list;
        SafeReadWriteExecutor m_safeExecutor;

        public SafeQueue() : this(new RWExecutor()) { }
        public SafeQueue(SafeReadWriteExecutor safeExecutor)
        {
            m_list = new LinkedList<T>();
            m_safeExecutor = safeExecutor;
        }

        public override T Read()
        {
            return m_safeExecutor.Execute(() => PopUnsafe());
        }

        public override void Write(T data)
        {
            m_safeExecutor.Execute(() => m_list.AddLast(data));
        }

        private T PopUnsafe()
        {
            if (m_list.Count < 1)
                return default(T);

            T returnVal = m_list.First.Value;
            m_list.RemoveFirst();
            return returnVal;
        }

        public int Count { get { return m_safeExecutor.ExecuteRead(() => m_list.Count); } }

        public void Clear()
        {
            m_safeExecutor.Execute(() => m_list.Clear());
        }
    }
}
