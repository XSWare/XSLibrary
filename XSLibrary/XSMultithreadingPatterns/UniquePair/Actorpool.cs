using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace XSLibrary.MultithreadingPatterns.UniquePair
{

    public partial class UniquePairThreading<PartType, GlobalDataType>
    {
        class ActorPool
        {
            public int PoolSize { get; private set; }
            public bool FixedCores { get; private set; }
            PairActor[] Actors { get; set; }
            ManualResetEvent[] ResetEvents { get; set; }

            public ActorPool(int size, bool fixedCores)
            {
                PoolSize = size;
                FixedCores = fixedCores;
                InitializeActors();
            }

            public void SendMessage(CalculationPair msg, int actorID)
            {
                ResetEvents[actorID].Reset();
                Actors[actorID].SendMessage(msg);
            }

            public void JoinThreads(int currentThreadCount)
            {
                WaitHandle.WaitAll(ResetEvents);
            }

            public void Close(bool join = false)
            {
                foreach (PairActor actor in Actors)
                {
                    actor.Stop();
                }

                if (join)
                    JoinThreads(PoolSize);
            }

            private void InitializeActors()
            {
                ResetEvents = new ManualResetEvent[PoolSize];
                Actors = new PairActor[PoolSize];
                for (int i = 0; i < PoolSize; i++)
                {
                    ResetEvents[i] = new ManualResetEvent(false);
                    Actors[i] = new PairActor(ResetEvents[i], FixedCores ? i : -1);
                }
            }
        }

        class PairActor
        {
            Thread m_thread;
            Semaphore m_lock;
            CalculationPair m_resource;
            volatile bool m_abort;
            ManualResetEvent m_resetEvent;
            int m_coreID;

            bool FixedCore { get { return m_coreID != -1; } }

            public PairActor(ManualResetEvent resetEvent) : this(resetEvent, -1) { }
            public PairActor(ManualResetEvent resetEvent, int coreID)
            {
                m_resetEvent = resetEvent;
                m_coreID = coreID;
                m_abort = false;
                m_lock = new Semaphore(0, 1);
                
                m_thread = new Thread(WorkLoop);
                m_thread.Start();
            }

            private void WorkLoop()
            {
                if (FixedCore)
                {
                    Thread.BeginThreadAffinity();
                    CurrentThread.ProcessorAffinity = new IntPtr(1 << m_coreID);
                }

                while (true)
                {
                    m_lock.WaitOne();

                    if (m_abort)
                    {
                        m_resetEvent.Set();
                        if(FixedCore)
                            Thread.EndThreadAffinity();
                        return;
                    }

                    m_resource.Calculate();
                    m_resource.Dispose();
                    m_resetEvent.Set();
                }
            }

            private static ProcessThread CurrentThread
            {
                get
                {
                    int id = DDLExports.GetCurrentThreadId();
                    return Process.GetCurrentProcess().Threads.Cast<ProcessThread>().Single(x => x.Id == id);
                }
            }

            public void SendMessage(CalculationPair pair)
            {
                m_resource = pair;
                m_resetEvent.Reset();
                m_lock.Release();
            }

            public void Stop()
            {
                m_abort = true;
                m_lock.Release();
            }
        }
    }
}
