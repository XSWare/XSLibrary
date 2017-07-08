using System;

namespace XSLibrary.Utility
{
    public abstract class TransparentFunctionWrapper
    {
        public ReturnType Execute<ReturnType>(Func<ReturnType> executeFunction)
        {
            ReturnType ret = default(ReturnType);
            Execute(new Action(() => ret = executeFunction()));
            return ret;
        }

        public abstract void Execute(Action executeFunction);
    }
}