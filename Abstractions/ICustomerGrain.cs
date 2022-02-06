using Orleans;

namespace WoodgroveBank.Abstractions
{
    public interface ICustomerGrain : IGrainWithIntegerKey
    {
        Task<Customer> GetCustomer();
        Task<Account[]> GetAccounts();
        Task<Transaction> GetTransactions();
    }
}
