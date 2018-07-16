using System;
using XSLibrary.ThreadSafety.Executors;

namespace XSLibrary.ThreadSafety
{
    // condition will only return true once, no matter how many threads access it
    public class OneShotCondition
    {
        volatile bool m_fired = false;
        SafeExecutor m_lock = new SingleThreadExecutor();

        Func<bool> m_externalCondition;

        public OneShotCondition() : this(() => { return true; }) { }
        public OneShotCondition(Func<bool> condition)
        {
            m_externalCondition = condition;
        }

        public static implicit operator bool(OneShotCondition condition)
        {
            return condition != null && condition.CheckCondition();
        }

        bool CheckCondition()
        {
            return m_lock.Execute(() =>
            {
                if (m_externalCondition() || m_fired)
                    return false;
                else
                {
                    m_fired = true;
                    return true;
                }
            });
        }
    }
}
