namespace XSLibrary.ThreadSafety.Locks
{
    public interface IReadWriteLock : ILock
    {
        void LockRead();
        void ReleaseRead();
        void UpgradeToWrite();
        void DowngradeToRead();
    }
}
