using System;
using XSLibrary.ThreadSafety.Locks;
using XSLibrary.Utility;

namespace XSLibrary.ThreadSafety.Executors
{
    public abstract class SafeExecutor : TransparentFunctionWrapper, IDisposable
    {
        protected ILock m_lock;

        protected SafeExecutor(ILock @lock)
        {
            m_lock = @lock;
        }

        override public void Execute(Action executeFunction)
        {
            try
            {
                m_lock.Lock();
                executeFunction();
            }
            finally { m_lock.Release(); }
        }

        public void Lock()
        {
            m_lock.Lock();
        }

        public void Release()
        {
            m_lock.Release();
        }

        public void Dispose()
        {
            m_lock.Dispose();
        }
    }
}