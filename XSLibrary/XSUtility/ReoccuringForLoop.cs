using System;

namespace XSLibrary.Utility
{
    public class ReoccuringForLoop
    {
        public int UpperLimit { get; set; }
        public int InitialIndex { get; set; }

        public ReoccuringForLoop (int upperLimit, int initialIndex = 0)
        {
            UpperLimit = upperLimit;
            InitialIndex = initialIndex;
        }

        public void Execute(Action<int> executeFunction)
        {
            for (int index = InitialIndex; index < UpperLimit; index++)
            {
                LoopActions(executeFunction, index);
            }
        }

        protected virtual void LoopActions(Action<int> executeFunction, int index)
        {
            executeFunction(index);
        }
    }
}
