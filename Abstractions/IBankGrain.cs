using Orleans;

namespace WoodgroveBank.Abstractions
{
    public interface IBankGrain : IGrainWithGuidKey
    {
        Task<Customer[]> GetCustomers();
        Task<Transaction[]> GetRecentTransactions();
        Task<bool> SubmitTransaction(Transaction transaction);
        Task UpdateCustomerIndex(Customer customer);
    }
}
