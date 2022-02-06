using Orleans;

namespace WoodgroveBank.Abstractions
{
    public interface IAccountGrain : IGrainWithIntegerKey
    {
        Task<Account> SaveAccount(Account account);
        Task<Account> GetAccount();
        Task<Account[]> FindAccounts(Func<Account, bool> query);
        Task<bool> Withdraw(decimal amount);
        Task<bool> Deposit(decimal amount);
    }
}