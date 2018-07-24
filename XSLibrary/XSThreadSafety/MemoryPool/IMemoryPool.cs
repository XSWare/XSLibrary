using System;
using System.Collections.Generic;
using XSLibrary.ThreadSafety.Executors;
using XSLibrary.Utility;

namespace XSLibrary.ThreadSafety.MemoryPool
{
    public abstract class IMemoryPool<Identifier, ElementType> where ElementType : IMemoryTransparentElement<Identifier>
    {
        class ReferencePair
        {
            public ElementType Element;
            public int ReferenceCount;
        }

        public Logger Logger { get; set; } = Logger.NoLog;

        Dictionary<Identifier, ReferencePair> m_referenceCounting = new Dictionary<Identifier, ReferencePair>();

        public SafeExecutor Lock { get; private set; } = new SingleThreadExecutor();

        public ElementType GetElement(Identifier ID)
        {
            return Lock.Execute(() =>
            {
                ReferencePair referencePair = FindPair(ID);
                referencePair.ReferenceCount++;
                return referencePair.Element;
            });
        }

        private ReferencePair FindPair(Identifier ID)
        {
            if (!m_referenceCounting.ContainsKey(ID))
            {
                ElementType element = CreateElement(ID, DecrementReferenceCount);
                m_referenceCounting.Add(ID, new ReferencePair() { Element = element, ReferenceCount = 0 });
                Logger.Log(LogLevel.Detail, "Allocated memory for element \"{0}\".", element.ID);
            }
            return m_referenceCounting[ID];
        }

        private void DecrementReferenceCount(Identifier ID)
        {
            Lock.Execute(() =>
            {
                ReferencePair referencePair = m_referenceCounting[ID];
                referencePair.ReferenceCount--;
                if (referencePair.ReferenceCount <= 0)
                {
                    Logger.Log(LogLevel.Detail, "Released memory of element \"{0}\".", ID);
                    m_referenceCounting.Remove(ID);
                }
            });
        }

        protected abstract ElementType CreateElement(Identifier ID, Action<Identifier> referenceCallback);
    }
}
