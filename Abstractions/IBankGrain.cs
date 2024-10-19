namespace WoodgroveBank.Abstractions
{
    public interface IBankGrain : IGrainWithGuidKey
    {
        Task<Customer[]> GetCustomers();
        Task LogTransaction(Transaction transaction);
        Task<Transaction[]> GetRecentTransactions();
        Task UpdateCustomerIndex(Customer customer);
    }
}
