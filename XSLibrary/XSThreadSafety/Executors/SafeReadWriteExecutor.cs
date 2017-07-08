using System;
using System.Threading;

namespace XSLibrary.ThreadSafety.Executors
{
    public abstract class SafeReadWriteExecutor : SafeExecutor
    {
        public virtual void ExecuteReadonly(Action executeFunction)
        {
            try
            {
                LockReadonly();
                executeFunction();
            }
            finally { ReleaseReadonly(); }
        }
        public virtual ReturnType ExecuteReadonly<ReturnType>(Func<ReturnType> executeFunction)
        {
            ReturnType ret = default(ReturnType);
            ExecuteReadonly(new Action(() => ret = executeFunction()));
            return ret;
        }

        public abstract void LockReadonly();
        public abstract void ReleaseReadonly();
    }
}