using System;

namespace XSLibrary.ThreadSafety.MemoryPool
{
    public abstract class IMemoryTransparentElement<Identifier> : IDisposable
    {
        public Identifier ID { get; private set; }

        public IMemoryTransparentElement(Identifier id)
        {
            ID = id;
        }

        public abstract bool IsEqual(Identifier ID);
        public abstract void Dispose();

    }
}
