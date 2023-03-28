using Orleans;
using Orleans.Runtime;
using WoodgroveBank.Abstractions;
using WoodgroveBank.Infrastructure;

namespace WoodgroveBank.Grains
{
    public class CustomerGrain : Grain, ICustomerGrain
    {
        private IPersistentState<Customer> _customerState { get; set; }
        private IPersistentState<List<Account>> _customerAccounts { get; set; }

        public CustomerGrain([PersistentState("customer", Strings.OrleansPersistenceNames.CustomerStore)] IPersistentState<Customer> customerState,
            [PersistentState("customerAccounts", Strings.OrleansPersistenceNames.CustomerAccountsStore)] IPersistentState<List<Account>> customerAccounts)
        {
            _customerState = customerState;
            _customerAccounts = customerAccounts;
        }

        public async Task<Customer> SaveCustomer(Customer customer)
        {
            _customerState.State = customer;
            await _customerState.WriteStateAsync();
            await _customerAccounts.WriteStateAsync();
            return _customerState.State;
        }

        public async Task<Customer> GetCustomer()
        {
            await _customerState.ReadStateAsync();
            await _customerAccounts.ReadStateAsync();
            return _customerState.State;
        }

        public async Task<Account> OpenAccount(Account account)
        {
            if (account.Id == Guid.Empty)
            {
                account.Id = Guid.NewGuid();
            }

            try
            {
                account.CustomerId = this.GetGrainId().GetGuidKey();
                var accountGrain = this.GrainFactory.GetGrain<IAccountGrain>(account.Id);
                account = await accountGrain.SaveAccount(account);

                await ReceiveAccountUpdate(account);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }


            return account;
        }

        public async Task<Account[]> GetAccounts()
        {
            await _customerAccounts.ReadStateAsync();
            return _customerAccounts.State.ToArray();
        }

        public async Task ReceiveAccountUpdate(Account account)
        {
            if (!_customerAccounts.State.Any(x => x.Id == account.Id))
            {
                _customerAccounts.State.Add(account);
            }
            else
            {
                _customerAccounts.State.First(x => x.Id == account.Id).Balance = account.Balance;
                await GrainFactory.GetGrain<IAccountGrain>(account.Id).SaveAccount(account);
            }

            await _customerAccounts.WriteStateAsync();
        }
    }
}
