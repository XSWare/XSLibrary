using System.Threading;
using XSLibrary.ThreadSafety.Containers;
using XSLibrary.Utility;

namespace XSLibrary.MultithreadingPatterns.Actor
{
    public abstract class Actor<MessageType>
    {
        public Logger Logger { get; set; }
        public string ThreadName { get; private set; }

        SafeQueue<MessageType> m_queue = new SafeQueue<MessageType>();
        Semaphore m_limit;
        Thread m_thread;
        bool m_abort;
        Semaphore m_creationLock = new Semaphore(1,1);

        public Actor(string threadName) : this(Logger.NoLog, threadName) { }
        public Actor() : this(Logger.NoLog) { }
        public Actor(Logger logger, string threadName = "Actor")
        {
            Logger = logger;
            ThreadName = threadName;
            Logger.Log(LogLevel.Information, "Actor created.");
            Initialize();
            StartThread();
        }

        private void Initialize()
        {
            m_abort = false;
            m_limit = new Semaphore(m_queue.Count, int.MaxValue);
        }

        private void StartThread()
        {
            m_thread = new Thread(WorkLoop);
            m_thread.Name = ThreadName;
            m_thread.Start();
            Logger.Log(LogLevel.Information, "Thread started. {0} messages in queue.", m_queue.Count);
        }

        public void Start(bool clearQueue = false)
        {
            new Thread(()=>RestartThread(clearQueue)).Start();
        }

        private void RestartThread(bool clearQueue)
        {
            m_creationLock.WaitOne();
            AbortThread(true);

            if(clearQueue)
                m_queue.Clear();

            Initialize();
            StartThread();
            m_creationLock.Release();
        }

        public void SendMessage(MessageType message)
        {
            m_queue.Write(message);
            m_limit.Release();
        }

        private void WorkLoop()
        {
            while (true)
            {
                m_limit.WaitOne();

                if(m_abort)
                {
                    Logger.Log(LogLevel.Information, "Thread aborted.");
                    return;
                }

                HandleMessage(m_queue.Read());
            }
        }

        abstract protected void HandleMessage(MessageType message);

        public void Stop(bool synchronized)
        {
            m_creationLock.WaitOne();
            AbortThread(synchronized);
            m_creationLock.Release();
        }

        /// <param name="synchronized">Wait until the actor stopped</param>
        private void AbortThread(bool synchronized)
        {
            m_abort = true;
            m_limit.Release();
            if(synchronized)
                m_thread.Join();
        }
    }
}
