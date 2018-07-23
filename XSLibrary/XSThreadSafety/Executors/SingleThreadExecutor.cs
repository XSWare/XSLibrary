using System.Threading;
using XSLibrary.ThreadSafety.Locks;

namespace XSLibrary.ThreadSafety.Executors
{
    public class SingleThreadExecutor : SafeExecutor
    {
        public SingleThreadExecutor() : base(new SingleLock()) { }
        public SingleThreadExecutor(Semaphore sharedLock) : this(new SingleLock(sharedLock)) { }
        public SingleThreadExecutor(SingleLock sharedLock) : base(sharedLock) { }
    }
}