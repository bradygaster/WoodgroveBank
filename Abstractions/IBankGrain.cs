using Orleans;

namespace WoodgroveBank.Abstractions
{
    public interface IBankGrain : IGrainWithGuidKey
    {
        Task<Customer[]> GetCustomers();
        Task<Transaction[]> GetRecentTransactions();
        Task UpdateCustomerIndex(Customer customer);
    }
}
