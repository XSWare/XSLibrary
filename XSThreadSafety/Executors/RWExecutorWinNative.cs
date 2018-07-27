using System.Threading;
using XSLibrary.ThreadSafety.Locks;

namespace XSLibrary.ThreadSafety.Executors
{
    public class RWExecutorWinNative : SafeReadWriteExecutor
    {
        public RWExecutorWinNative() : this(new ReaderWriterLock()) { }
        public RWExecutorWinNative(ReaderWriterLock sharedLock) : this(new RWWinNative(sharedLock)) { }
        public RWExecutorWinNative(RWWinNative sharedLock) : base(sharedLock) { }
    }
}
