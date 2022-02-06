using Orleans;

namespace WoodgroveBank.Abstractions
{
    public interface IBankGrain : IGrainWithGuidKey
    {
        Task SaveCustomer(Customer customer);
        Task<Customer[]> GetCustomers();
        Task<Account[]> GetAccounts();
        Task<Account> OpenAccount(string name, Customer customer, AccountType accountType, decimal amount);
        Task<Transaction[]> GetRecentTransactions();
        Task<bool> SubmitTransaction(Transaction transaction);
    }
}
