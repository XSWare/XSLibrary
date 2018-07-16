using System;
using System.Threading;
using XSLibrary.ThreadSafety.Containers;

namespace XSLibrary.ThreadSafety
{
    public class CatchingThread
    {
        Thread _thread;

        public DataContainer<Exception> Exceptions { get; private set; }

        public CatchingThread(ThreadStart threadStart, DataContainer<Exception> exceptionContainer)
        {
            _thread = new Thread(() => SafeExecution(threadStart));
        }

        private void SafeExecution(ThreadStart threadStart)
        {
            try { threadStart.Invoke(); }
            catch (Exception ex)
            {
                Exceptions.Write(ex);
            }
        }

        public void Start() => _thread.Start();
        public void Join() => _thread.Join();
        public bool IsAlive => _thread.IsAlive;
        public ThreadState ThreadState => _thread.ThreadState;
        public string Name
        {
            get { return _thread.Name; }
            set { _thread.Name = value; }
        }
    }
}
