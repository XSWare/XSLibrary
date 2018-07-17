using XSLibrary.ThreadSafety.Executors;

namespace XSLibrary.ThreadSafety
{
    /// <summary>
    /// Triggers once, if the event is invoked or was invoked before subscribing to it.
    /// Do not unsubscribe from this event in the event handler, it happens automatically.
    /// </summary>
    public class OneTimeEvent
    {
        public delegate void FireHandle();

        public event FireHandle OnFire
        {
            add
            {
                m_lock.Execute(() =>
                {
                    if (m_invoked)
                        value();
                    else
                        InternalEvent += value;
                });
            }
            remove { InternalEvent -= value; }
        }

        private SafeExecutor m_lock = new SingleThreadExecutor();
        private volatile bool m_invoked = false;

        private event FireHandle InternalEvent;

        public void Invoke()
        {
            m_lock.Execute(() =>
            {
                m_invoked = true;
                InternalEvent?.Invoke();
                InternalEvent = null;
            });
        }
    }
}
