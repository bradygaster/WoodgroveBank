using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoodgroveBank.Abstractions;
using WoodgroveBank.Infrastructure;

namespace WoodgroveBank.Grains
{
    public class CustomerGrain : Grain, ICustomerGrain
    {
        private IPersistentState<List<Customer>> _customerListState { get; set; }

        public CustomerGrain([PersistentState("customers", Strings.OrleansPersistenceNames.CustomersStore)] 
            IPersistentState<List<Customer>> customerListState)
        {
            _customerListState = customerListState;
        }

        public async Task<Customer> SaveCustomer(Customer customer)
        {
            if (customer.Id == 0) // new customer
            {
                customer.Id = new Random().Next(1000, 9999);
                _customerListState.State.Add(customer);
            }
            else if (!_customerListState.State.Any(x => x.Id == customer.Id)) // unrecognized customer id
            {
                throw new Exception("Customer doesn't match any of our known customers.");
            }
            else // existing customer to update
            {
                _customerListState.State.First(x => x.Id == customer.Id).City = customer.City;
                _customerListState.State.First(x => x.Id == customer.Id).Country = customer.Country;
                _customerListState.State.First(x => x.Id == customer.Id).Name = customer.Name;
            }

            await _customerListState.WriteStateAsync();

            return customer;
        }

        public async Task<Customer> GetCustomer()
        {
            var customerId = this.GetGrainIdentity().PrimaryKeyLong;
            var customers = await GetCustomers();
            var customer = customers.First(x => x.Id == customerId);
            return customer;
        }

        public async Task<Customer[]> GetCustomers()
        {
            await _customerListState.ReadStateAsync();
            return _customerListState.State.ToArray();
        }

        public Task<Account[]> GetAccounts()
        {
            throw new NotImplementedException();
        }

        public Task<Transaction> GetTransactions()
        {
            throw new NotImplementedException();
        }
    }
}
