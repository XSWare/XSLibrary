using System;

namespace XSLibrary.ThreadSafety.Locks
{
    public interface ILock : IDisposable
    {
        void Lock();
        void Release();
    }
}