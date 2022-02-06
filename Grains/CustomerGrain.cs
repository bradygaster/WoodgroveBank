using Orleans;
using Orleans.Runtime;
using WoodgroveBank.Abstractions;
using WoodgroveBank.Infrastructure;

namespace WoodgroveBank.Grains
{
    public class CustomerGrain : Grain, ICustomerGrain
    {
        private IPersistentState<Customer> _customerState { get; set; }

        public CustomerGrain([PersistentState("customer", Strings.OrleansPersistenceNames.CustomerStore)] 
            IPersistentState<Customer> customerState)
        {
            _customerState = customerState;
        }

        public async Task<Customer> SaveCustomer(Customer customer)
        {
            _customerState.State = customer;
            await _customerState.WriteStateAsync();
            return _customerState.State;
        }

        public async Task<Customer> GetCustomer()
        {
            await _customerState.ReadStateAsync();
            return _customerState.State;
        }

        public async Task<Account> OpenAccount(Account account)
        {
            if(account.Id == 0) // new account
            {
                account.Id = new Random().Next(10000, 99999);
                account.CustomerId = this.GetGrainIdentity().PrimaryKeyLong;
            }

            var accountGrain = this.GrainFactory.GetGrain<IAccountGrain>(account.Id);
            account = await accountGrain.SaveAccount(account);

            return account;
        }
    }
}
