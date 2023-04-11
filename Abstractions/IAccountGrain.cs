namespace WoodgroveBank.Abstractions
{
    public interface IAccountGrain : IGrainWithGuidKey
    {
        Task<decimal> GetBalance();
        Task<Account> SaveAccount(Account account);
        Task<bool> Withdraw(decimal amount);
        Task<bool> Deposit(decimal amount);
        Task<Transaction[]> GetTransactions();
    }
}