using Orleans;

namespace WoodgroveBank.Abstractions
{
    public interface IAccountGrain : IGrainWithIntegerKey
    {
        Task<Account> GetAccount();
        Task<decimal> GetAccountBalance();
        Task<bool> Withdraw(decimal amount);
        Task<bool> Deposit(decimal amount);
        Task UpdateAccountStatus(AccountStatus accountStatus);
    }
}