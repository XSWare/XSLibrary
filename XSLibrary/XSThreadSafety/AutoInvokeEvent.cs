using XSLibrary.ThreadSafety.Executors;

namespace XSLibrary.ThreadSafety
{
    /// <summary>
    /// Triggers if the event is invoked or was invoked before subscribing to it.
    /// Unsubscribing happens automatically after the invocation.
    /// </summary>
    public class AutoInvokeEvent<Sender, Args>
    {
        public delegate void EventHandle(Sender sender, Args arguments);

        public event EventHandle Event
        {
            add
            {
                m_lock.Execute(() =>
                {
                    if (m_invoked)
                        value(m_sender, m_eventArgs);
                    else
                        InternalEvent += value;
                });
            }
            remove { InternalEvent -= value; }
        }

        private SafeExecutor m_lock = new SingleThreadExecutor();
        private volatile bool m_invoked = false;

        private event EventHandle InternalEvent;

        Sender m_sender;
        Args m_eventArgs;

        public void Invoke(Sender sender, Args e)
        {
            m_lock.Execute(() =>
            {
                if (m_invoked)
                    return;

                m_sender = sender;
                m_eventArgs = e;
                m_invoked = true;
                InternalEvent?.Invoke(m_sender, m_eventArgs);
                InternalEvent = null;
            });
        }
    }
}
