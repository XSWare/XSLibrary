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
                if (!Subscribe(value))
                    value(m_sender, m_eventArgs);
            }
            remove { InternalEvent -= value; }
        }

        private SafeExecutor m_lock = new SingleThreadExecutor();
        private volatile bool m_invoked = false;

        private event EventHandle InternalEvent;

        Sender m_sender;
        Args m_eventArgs;

        public void Invoke(Sender sender, Args args)
        {
            GetEventHandle(sender, args)?.Invoke(m_sender, m_eventArgs);
        }

        private EventHandle GetEventHandle(Sender sender, Args args)
        {
            return m_lock.Execute(() =>
            {
                if (m_invoked)
                    return null;

                m_sender = sender;
                m_eventArgs = args;
                m_invoked = true;
                EventHandle handle = InternalEvent;
                InternalEvent = null;
                return handle;
            });
        }

        /// <returns>Returns true if subscription was successful and false if handle needs to be called immediately.</returns>
        private bool Subscribe(EventHandle handle)
        {
            return m_lock.Execute(() =>
            {
                if (!m_invoked)
                    InternalEvent += handle;

                return !m_invoked;
            });
        }
    }
}
