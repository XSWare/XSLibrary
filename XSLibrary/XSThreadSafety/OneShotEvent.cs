using System;
using XSLibrary.ThreadSafety.Executors;

namespace XSLibrary.ThreadSafety
{
    /// <summary>
    /// Triggers once, if the event is invoked or was invoked before subscribing to it.
    /// Do not unsubscribe from this event in the event handler, it happens automatically.
    /// </summary>
    public class OneTimeEvent
    {
        public delegate void EventHandle(object sender, EventArgs e);

        public event EventHandle OnEventRaise
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

        object m_sender;
        EventArgs m_eventArgs;

        public void Invoke(object sender, EventArgs e)
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
