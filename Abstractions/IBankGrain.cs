namespace WoodgroveBank.Abstractions
{
    public interface IBankGrain : IGrainWithGuidKey
    {
        Task<Customer[]> GetCustomers();
        Task LogTransaction(AccountTransaction transaction);
        Task<AccountTransaction[]> GetRecentTransactions();
        Task UpdateCustomerIndex(Customer customer);
        Task<BankSettings> GetSettings();
        Task SaveSettings(BankSettings settings);
    }
}
