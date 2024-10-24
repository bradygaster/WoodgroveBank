namespace WoodgroveBank.Web.Grains;

public class BankGrain(
        [PersistentState("customers", "grainState")] IPersistentState<List<Customer>> customerIndex,
        [PersistentState("transactions", "grainState")] IPersistentState<List<Transaction>> transactionHistory,
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

    public Task<Transaction[]> GetRecentTransactions()
    {
        return Task.FromResult(transactionHistory.State.OrderByDescending(x => x.Timestamp).Take(10).ToArray());
    }

    public async Task LogTransaction(Transaction transaction)
    {
        transactionProcessedStreamHandler.OnTransactionReceived(new TransactionProcessedEventArgs
        {
            Transaction = transaction,
            Customer = customerIndex.State.First(x => x.Id == transaction.CustomerId)
        });

        transactionHistory.State.Add(transaction);
        await transactionHistory.WriteStateAsync();
    }
}
