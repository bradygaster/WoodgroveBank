using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using WoodgroveBank.Abstractions;

namespace WoodgroveBank.Grains
{
    public class BankGrain : Grain, IBankGrain
    {
        private IPersistentState<List<Transaction>> _transactionListState;
        private IPersistentState<List<Customer>> _customerIndex;
        private readonly ILogger<BankGrain> _logger;

        public BankGrain(
            [PersistentState("customers")] IPersistentState<List<Customer>> customerIndex,
            [PersistentState("transactions")] IPersistentState<List<Transaction>> transactionListState,
            ILogger<BankGrain> logger)
        {
            _customerIndex = customerIndex;
            _transactionListState = transactionListState;
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
            var streamProvider = this.GetStreamProvider("ADMIN");
            var recentCustomerStreamId = StreamId.Create("ADMIN", "RECENT_CUSTOMERS");
            var stream = streamProvider.GetStream<Customer>(recentCustomerStreamId);
            stream.OnNextAsync(customer);
        }

        public Task<Transaction[]> GetRecentTransactions()
        {
            return Task.FromResult(_transactionListState.State.OrderBy(x => x.Timestamp).ToArray());
        }

        public async Task<Customer> AuthenticateCustomer(string pin)
        {
            await _customerIndex.ReadStateAsync();
            return _customerIndex.State.FirstOrDefault(x => x.Pin == pin);
        }
    }
}
