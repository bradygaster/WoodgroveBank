using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Utilities;
using WoodgroveBank.Abstractions;

namespace WoodgroveBank.Grains
{
    public class BankGrain : Grain, IBankGrain
    {
        private IPersistentState<List<Transaction>> _transactionListState;
        private IPersistentState<List<Customer>> _customerIndex;
        private readonly ObserverManager<IAdminDashboardObserver> _observers;
        private readonly ILogger<BankGrain> _logger;

        public BankGrain(
            [PersistentState("customers")] IPersistentState<List<Customer>> customerIndex,
            [PersistentState("transactions")] IPersistentState<List<Transaction>> transactionListState,
            ILogger<BankGrain> logger)
        {
            _customerIndex = customerIndex;
            _transactionListState = transactionListState;
            _observers = new ObserverManager<IAdminDashboardObserver>(TimeSpan.FromSeconds(60), _logger);
            _logger = logger;
        }
        public async Task<Customer[]> GetCustomers()
        {
            await _customerIndex.ReadStateAsync();
            return _customerIndex.State.ToArray();
        }
        public async Task UpdateCustomerIndex(Customer customer)
        {
            await _customerIndex.ReadStateAsync();
            if (!_customerIndex.State.Any(x => x.Id == customer.Id))
            {
                _customerIndex.State.Add(customer);
                await _customerIndex.WriteStateAsync();
            }

            await _observers.Notify(_ => _.OnCustomerIndexUpdated(customer));
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

        public Task Subscribe(IAdminDashboardObserver observer)
        {
            if (observer == null)
                _logger.LogWarning("Observer is null");

            if (_observers == null)
                _logger.LogWarning("_observers is null");

            if (observer != null && _observers != null)
            {
                _logger.LogInformation("Subscribing...");

                try
                {
                    _observers.Subscribe(observer, observer);
                    _logger.LogInformation("Subscribed");
                }
                catch(Exception ex)
                {
                    _logger.LogError("Error during subscribing", ex);
                }
            }

            return Task.CompletedTask;
        }

        public Task Unsubscribe(IAdminDashboardObserver observer)
        {
            if (observer != null)
                _observers.Unsubscribe(observer);
            return Task.CompletedTask;
        }
    }
}
