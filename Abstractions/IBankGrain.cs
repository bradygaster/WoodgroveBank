using Orleans;

namespace WoodgroveBank.Abstractions
{
    public interface IBankGrain : IGrainWithGuidKey
    {
        Task<Account> OpenAccount(string name, Customer customer, AccountType accountType, decimal amount);
        Task<Transaction[]> GetRecentTransactions();
        Task<bool> SubmitTransaction(Transaction transaction);
    }
}
