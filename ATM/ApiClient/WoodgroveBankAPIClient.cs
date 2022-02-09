using Refit;
using WoodgroveBank.Abstractions;

namespace ATM.ApiClient
{
    public class WoodgroveBankAPIClient : IWoodgroveBankApi
    {
        HttpClient _client;

        public WoodgroveBankAPIClient(IHttpClientFactory clientFactory)
        {
            _client = clientFactory.CreateClient("api");
        }

        public async Task CreateAccount(Account account)
        {
            await RestService.For<IWoodgroveBankApi>(_client).CreateAccount(account);
        }

        public async Task CreateCustomer(Customer customer)
        {
            await RestService.For<IWoodgroveBankApi>(_client).CreateCustomer(customer);
        }

        public async Task<Customer[]> GetCustomers()
        {
            return await RestService.For<IWoodgroveBankApi>(_client).GetCustomers();
        }

        public async Task<Customer> SignIn(string customerPin)
        {
            Customer result = null;
            try
            {
                result = await RestService.For<IWoodgroveBankApi>(_client).SignIn(customerPin);
            }
            catch(Exception ex)
            {
                return null;
            }
            return result;
        }
    }

    public interface IWoodgroveBankApi
    {
        [Get("/customers")]
        Task<Customer[]> GetCustomers();

        [Post("/customers")]
        Task CreateCustomer(Customer customer);

        [Post("/accounts")]
        Task CreateAccount(Account account);

        [Get("/atm/signin/{customerPin}")]
        Task<Customer> SignIn(string customerPin);
    }
}
