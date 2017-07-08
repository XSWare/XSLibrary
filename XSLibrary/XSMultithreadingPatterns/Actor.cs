using System.Threading;
using XSLibrary.ThreadSafety.Containers;
using XSLibrary.Utility;

namespace XSLibrary.MultithreadingPatterns.Actor
{
    public abstract class Actor<MessageType>
    {
        public Logger Logger { get; set; }

        SafeQueue<MessageType> m_queue = new SafeQueue<MessageType>();
        Semaphore m_limit;
        Thread m_thread;
        bool m_abort;
        Semaphore m_creationLock = new Semaphore(1,1);


        public Actor(Logger logger)
        {
            Logger = logger;
            Logger.Log("Actor started.");
            Initialize();
            StartThread();
        }
        public Actor() : this(new NoLog())
        {
        }

        private void Initialize()
        {
            m_abort = false;
            m_limit = new Semaphore(m_queue.Count, 1000000);
        }

        private void StartThread()
        {
            m_thread = new Thread(WorkLoop);
            m_thread.Start();
            Logger.Log("Thread started. {0} messages in queue.", m_queue.Count);
        }

        public void Start(bool clearQueue = false)
        {
            new Thread(()=>RestartThread(clearQueue)).Start();
        }

        private void RestartThread(bool clearQueue)
        {
            m_creationLock.WaitOne();
            AbortThread();

            if(clearQueue)
                m_queue.Clear();

            Initialize();
            StartThread();
            m_creationLock.Release();
        }

        public void SendMessage(MessageType mes)
        {
            m_queue.Add(mes);
            m_limit.Release();
        }

        private void WorkLoop()
        {
            while (true)
            {
                m_limit.WaitOne();

                if(m_abort)
                {
                    Logger.Log("Thread aborted.");
                    return;
                }

                HandleMessage(m_queue.Pop());
            }
        }

        abstract protected void HandleMessage(MessageType message);

        public void Stop()
        {
            m_creationLock.WaitOne();
            AbortThread();
            m_creationLock.Release();
        }

        private void AbortThread()
        {
            m_abort = true;
            m_limit.Release();
            m_thread.Join();
        }
    }
}
