using System;

namespace XSLibrary.ThreadSafety.MemoryPool
{
    public abstract class IMemoryTransparentElement<Identifier> : IDisposable
    {
        public Identifier ID { get; private set; }

        Action<Identifier> DecrementCallback;

        public IMemoryTransparentElement(Identifier id, Action<Identifier> referenceCallback)
        {
            ID = id;
            DecrementCallback = referenceCallback;
        }

        public void DecrementReferenceCount()
        {
            DecrementCallback(ID);
        }

        public abstract bool IsEqual(Identifier ID);
        public abstract void Dispose();

    }
}
