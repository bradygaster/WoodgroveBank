namespace WoodgroveBank.Abstractions
{
    public interface IBankGrain : IGrainWithGuidKey
    {
        Task<Customer[]> GetCustomers();
        Task<Transaction[]> GetRecentTransactions();
        Task UpdateCustomerIndex(Customer customer);
        Task<Customer> AuthenticateCustomer(string pin);
        Task Subscribe(IAdminDashboardObserver observer);
        Task Unsubscribe(IAdminDashboardObserver observer);
    }
}
