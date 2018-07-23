using System;
using XSLibrary.ThreadSafety.Locks;

namespace XSLibrary.ThreadSafety.Executors
{
    public abstract class SafeReadWriteExecutor : SafeExecutor
    {
        IReadWriteLock RWLock
        {
            get => m_lock as IReadWriteLock;
            set => m_lock = value;
        }

        protected SafeReadWriteExecutor(IReadWriteLock sharedLock) : base(sharedLock) { }

        public virtual void ExecuteRead(Action executeFunction)
        {
            try
            {
                RWLock.LockRead();
                executeFunction();
            }
            finally { RWLock.ReleaseRead(); }
        }
        public virtual ReturnType ExecuteRead<ReturnType>(Func<ReturnType> executeFunction)
        {
            ReturnType ret = default(ReturnType);
            ExecuteRead(new Action(() => ret = executeFunction()));
            return ret;
        }

        public void LockRead()
        {
            RWLock.LockRead();
        }

        public void ReleaseRead()
        {
            RWLock.ReleaseRead();
        }

        public void UpgradeToWrite()
        {
            RWLock.UpgradeToWrite();
        }

        public void DowngradeToRead()
        {
            RWLock.DowngradeToRead();
        }
    }
}