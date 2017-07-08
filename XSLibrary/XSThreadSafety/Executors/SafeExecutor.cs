using System;
using System.Threading;
using XSLibrary.Utility;

namespace XSLibrary.ThreadSafety.Executors
{
    public abstract class SafeExecutor : TransparentFunctionWrapper
    {
        override public void Execute(Action executeFunction)
        {
            try
            {
                Lock();
                executeFunction();
            }
            finally { Release(); }
        }

        public abstract void Lock();
        public abstract void Release();
    }
}