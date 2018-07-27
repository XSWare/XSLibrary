using XSLibrary.ThreadSafety.MemoryPool;

namespace XSLibrary.Network.Registrations
{
    public abstract class IAccountPool<AccountType> : IMemoryPool<string, AccountType> where AccountType : IUserAccount
    {
        public int AccessLevel { get; private set; }

        protected IAccountPool(int accessLevel)
        {
            AccessLevel = accessLevel;
        }
    }
}
