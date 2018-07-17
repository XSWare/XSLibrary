using XSLibrary.ThreadSafety.Executors;

namespace XSLibrary.ThreadSafety
{
    public class OneShotEvent
    {
        enum EventStates
        {
            Untouched,
            Invoked,
            Fired
        }

        public delegate void FireHandle();

        public event FireHandle OnFire
        {
            add
            {
                m_lock.Execute(() =>
                {
                    InternalEvent += value;
                    if(state == EventStates.Invoked)
                        FireEvent();
                });
            }
            remove { m_lock.Execute(() => InternalEvent -= value); }
        }

        private SafeExecutor m_lock = new SingleThreadExecutor();
        private volatile EventStates state = EventStates.Untouched;

        private event FireHandle InternalEvent;

        public OneShotEvent()
        {
        }

        public void Invoke()
        {
            m_lock.Execute(() =>
            {
                if(state == EventStates.Untouched)
                    state = EventStates.Invoked;
                FireEvent();
            });
        }

        private void FireEvent()
        {
            if (state != EventStates.Fired && InternalEvent != null)
            {
                state = EventStates.Fired;
                InternalEvent();
            }
        }
    }
}
