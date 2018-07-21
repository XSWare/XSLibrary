using XSLibrary.Utility;

namespace XSLibrary.Network.Registrations
{
    public abstract class IUserAccount
    {
        public delegate void MemoryReleaseHandler(object sender);
        public abstract event MemoryReleaseHandler OnMemoryCleanUp;

        public Logger Logger { get; set; } = Logger.NoLog;
        public string Username { get; private set; }

        public IUserAccount(string username)
        {
            Username = username;
        }

        public abstract bool StillInUse();
    }
}
