namespace WoodgroveBank.Web.Grains;

public class BankGrain(
        [PersistentState("customers", "grainState")] IPersistentState<List<Customer>> customerIndex,
        [PersistentState("transactions", "grainState")] IPersistentState<List<AccountTransaction>> transactionHistory,
        [PersistentState("bankSettings", "grainState")] IPersistentState<BankSettings> bankSettings,
        CustomerReceivedStreamHandler customerReceivedStreamHandler,
        TransactionProcessedStreamHandler transactionProcessedStreamHandler,
        ILogger<BankGrain> logger) : Grain, IBankGrain
{    
    public async Task<Customer[]> GetCustomers()
    {
        await customerIndex.ReadStateAsync();
        return customerIndex.State.ToArray();
    }

    public async Task UpdateCustomerIndex(Customer customer)
    {
        if (!customerIndex.State.Any(x => x.Id == customer.Id))
        {
            customerIndex.State.Add(customer);
            await customerIndex.WriteStateAsync();
        }

        customerReceivedStreamHandler.OnCustomerReceived(customer);
    }

    public Task<AccountTransaction[]> GetRecentTransactions()
    {
        return Task.FromResult(transactionHistory.State.OrderByDescending(x => x.Timestamp).Take(10).ToArray());
    }

    public async Task LogTransaction(AccountTransaction transaction)
    {
        transactionProcessedStreamHandler.OnTransactionReceived(new TransactionProcessedEventArgs
        {
            Transaction = transaction,
            Customer = customerIndex.State.First(x => x.Id == transaction.CustomerId)
        });

        transactionHistory.State.Add(transaction);
        await transactionHistory.WriteStateAsync();
    }

    public async Task<BankSettings> GetSettings()
    {
        await bankSettings.ReadStateAsync();
        return bankSettings.State;
    }

    public async Task SaveSettings(BankSettings settings)
    {
        bankSettings.State = settings;
        await bankSettings.WriteStateAsync();
    }
}
