using Orleans;

namespace WoodgroveBank.Abstractions
{
    public interface ICustomerGrain : IGrainWithIntegerKey
    {
        Task<Customer> SaveCustomer(Customer customer);
        Task<Customer[]> GetCustomers();
        Task<Customer> GetCustomer();
        Task<Account[]> GetAccounts();
        Task<Transaction> GetTransactions();
    }
}
