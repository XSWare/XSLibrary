using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace XSLibrary.MultithreadingPatterns.UniquePair.DistributionNodes
{
    public partial class ActorPool<PartType, GlobalDataType> : SharedMemoryPool<PartType, GlobalDataType>
    {
        public class ActorNode : DistributionNode
        {
            Thread m_thread;
            Semaphore m_lock;
            CalculationPair<PartType, GlobalDataType> m_resource;
            volatile bool m_abort;
            ManualResetEvent m_resetEvent;
            int m_coreID;
            SharedMemoryStackCalculation<PartType, GlobalDataType> m_calculationAlogrithm;

            bool FixedCore { get { return m_coreID != -1; } }

            public ActorNode(SharedMemoryStackCalculation<PartType, GlobalDataType> calculationHandler, ManualResetEvent resetEvent) : this(calculationHandler, resetEvent, -1) { }
            public ActorNode(SharedMemoryStackCalculation<PartType, GlobalDataType> calculationHandler, ManualResetEvent resetEvent, int coreID)
            {
                m_calculationAlogrithm = calculationHandler;

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
                        if (FixedCore)
                            Thread.EndThreadAffinity();
                        return;
                    }
                    m_calculationAlogrithm.Calculate(m_resource);
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

            public override void CalculateStacks(CalculationPair<PartType, GlobalDataType> calculationPair)
            {
                m_resource = calculationPair;
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
