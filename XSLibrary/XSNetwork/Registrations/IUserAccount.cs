using System;
using XSLibrary.ThreadSafety.MemoryPool;
using XSLibrary.Utility;

namespace XSLibrary.Network.Registrations
{
    public abstract class IUserAccount : IMemoryTransparentElement<string>
    {
        public Logger Logger { get; set; } = Logger.NoLog;
        public string Username => ID;

        public IUserAccount(string username, Action<string> referenceCallback) : base(username, referenceCallback)
        {
        }
    }
}