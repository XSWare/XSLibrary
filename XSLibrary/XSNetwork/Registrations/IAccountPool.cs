using System.Collections.Generic;
using XSLibrary.ThreadSafety.Executors;
using XSLibrary.Utility;

namespace XSLibrary.Network.Registrations
{
    public abstract class IAccountPool<AccountType> where AccountType : IUserAccount
    {
        public Logger Logger { get; set; } = Logger.NoLog;

        public SafeExecutor Lock { get; private set; } = new SingleThreadExecutor();
        List<AccountType> Accounts { get; set; } = new List<AccountType>();

        public void AddAccount(AccountType account)
        {
            Lock.Execute(() =>
            {
                if(!Accounts.Contains(account))
                    Accounts.Add(account);
            });
        }

        public AccountType GetAccount(string username)
        {
            return Lock.Execute(() =>
            {
                AccountType user = null;

                foreach (AccountType account in Accounts)
                {
                    if (account.Username == username)
                        return account;
                }

                if (user != null)
                    return user;

                user = CreateAccount(username);
                Logger.Log(LogLevel.Detail, "Allocated memory for account \"{0}\".", user.Username);
                user.Logger = Logger;
                user.OnMemoryCleanUp += DisposeAccount;
                Accounts.Add(user);
                return user;
            });
        }

        public void DisposeAccount(object account)
        {
            Lock.Execute(() =>
            {
                AccountType user = account as AccountType;
                if (user != null && !user.StillInUse())
                    Accounts.Remove(user);
            });
        }

        protected abstract AccountType CreateAccount(string username);
    }
}
