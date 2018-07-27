using XSLibrary.ThreadSafety.Locks;

namespace XSLibrary.ThreadSafety.Executors
{
    public class RWExecutor : SafeReadWriteExecutor
    {
        public RWExecutor() : this(new RWLock()) { }
        public RWExecutor(RWLock sharedLock) : base(sharedLock) { }
    }
}