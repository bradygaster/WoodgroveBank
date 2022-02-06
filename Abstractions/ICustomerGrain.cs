using Orleans;

namespace WoodgroveBank.Abstractions
{
    public interface ICustomerGrain : IGrainWithGuidKey
    {
        Task<Customer> SaveCustomer(Customer customer);
        Task<Customer> GetCustomer();
        Task<Account> OpenAccount(Account account);
    }
}
