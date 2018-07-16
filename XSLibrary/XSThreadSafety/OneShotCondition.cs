using System;
using XSLibrary.ThreadSafety.Executors;

namespace XSLibrary.ThreadSafety
{
    /// <summary>
    /// Condition will only return true once, no matter how many threads access it
    /// </summary>
    public class OneShotCondition
    {
        volatile bool m_fired = false;
        SafeExecutor m_lock = new SingleThreadExecutor();

        Func<bool> m_permanentCondition;

        public OneShotCondition() : this(True) { }

        /// <param name="permanentAdditionalCondition">Condition will be checked on every call before firing</param>
        public OneShotCondition(Func<bool> permanentAdditionalCondition)
        {
            m_permanentCondition = permanentAdditionalCondition;
        }

        public static implicit operator bool(OneShotCondition condition)
        {
            return condition != null && condition.Fire();
        }

        public bool Fire()
        {
            return Fire(True);
        }

        /// <param name="temporaryAdditionalCondition">Condition will be checked as well on this call before firing</param>
        public bool Fire(Func<bool> temporaryAdditionalCondition)
        {
            return m_lock.Execute(() =>
            {
                if (m_fired || !m_permanentCondition() || !temporaryAdditionalCondition())
                    return false;
                else
                {
                    m_fired = true;
                    return true;
                }
            });
        }

        private static bool True()
        {
            return true;
        }
    }
}
