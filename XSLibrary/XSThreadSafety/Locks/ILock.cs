namespace XSLibrary.ThreadSafety.Locks
{
    public interface ILock
    {
        void Lock();
        void Release();
    }
}