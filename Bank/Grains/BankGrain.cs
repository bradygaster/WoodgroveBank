namespace WoodgroveBank.Web.Grains;

public class BankGrain : Grain, IBankGrain
{
    private IPersistentState<List<Transaction>> _transactionHistory;
    private IPersistentState<List<Customer>> _customerIndex;
    private readonly ILogger<BankGrain> _logger;

    public BankGrain(
        [PersistentState("customers", "grainState")] IPersistentState<List<Customer>> customerIndex,
        [PersistentState("transactions", "grainState")] IPersistentState<List<Transaction>> transactionHistory,
        ILogger<BankGrain> logger)
    {
        _customerIndex = customerIndex;
        _transactionHistory = transactionHistory;
        _logger = logger;
    }
    
    public async Task<Customer[]> GetCustomers()
    {
        await _customerIndex.ReadStateAsync();
        return _customerIndex.State.ToArray();
    }

    public async Task UpdateCustomerIndex(Customer customer)
    {
        if (!_customerIndex.State.Any(x => x.Id == customer.Id))
        {
            _customerIndex.State.Add(customer);
            await _customerIndex.WriteStateAsync();
        }

        // push the update to the steam
        var streamProvider = this.GetStreamProvider("BANK");
        var recentCustomerStreamId = StreamId.Create("BANK", "RECENT_CUSTOMERS");
        var stream = streamProvider.GetStream<Customer>(recentCustomerStreamId);
        stream.OnNextAsync(customer);
    }

    public Task<Transaction[]> GetRecentTransactions()
    {
        return Task.FromResult(_transactionHistory.State.OrderByDescending(x => x.Timestamp).Take(10).ToArray());
    }

    public async Task LogTransaction(Transaction transaction)
    {
        // push the update to the steam
        var streamProvider = this.GetStreamProvider("BANK");
        var recentTransactionStreamId = StreamId.Create("BANK", "RECENT_TRANSACTIONS");
        var stream = streamProvider.GetStream<Transaction>(recentTransactionStreamId);
        stream.OnNextAsync(transaction);

        _transactionHistory.State.Add(transaction);
        await _transactionHistory.WriteStateAsync();
    }
}
