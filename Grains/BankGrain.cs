using Orleans;
using Orleans.Runtime;
using WoodgroveBank.Abstractions;
using WoodgroveBank.Infrastructure;

namespace WoodgroveBank.Grains
{
    public class BankGrain : Grain, IBankGrain
    {
        private IPersistentState<List<Transaction>> _transactionListState { get; set; }
        private IPersistentState<List<Customer>> _customerIndex { get; set; }

        public BankGrain(
            [PersistentState("customers", Strings.OrleansPersistenceNames.CustomersStore)] IPersistentState<List<Customer>> customerIndex,
            [PersistentState("transactions", Strings.OrleansPersistenceNames.TransactionsStore)] IPersistentState<List<Transaction>> transactionListState)
        {
            _customerIndex = customerIndex;
            _transactionListState = transactionListState;
        }
        public async Task<Customer[]> GetCustomers()
        {
            await _customerIndex.ReadStateAsync();
            return _customerIndex.State.ToArray();
        }
        public async Task UpdateCustomerIndex(Customer customer)
        {
            await _customerIndex.ReadStateAsync();
            if(!_customerIndex.State.Any(x => x.Id == customer.Id))
            {
                _customerIndex.State.Add(customer);
                await _customerIndex.WriteStateAsync();
            }
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
