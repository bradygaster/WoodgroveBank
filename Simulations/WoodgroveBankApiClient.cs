using Refit;

namespace Simulations;

/// <summary>
/// Whilst the Simulations project has access to the Orleans cluster,
/// this interface is used to interact with the WoodgroveBank HTTP API
/// in much the same way a client application would that's outside
/// of the bank cluster.
/// </summary>
public interface IWoodgroveBankApiClient
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

public class WoodgroveBankApiClient(HttpClient httpClient) : IWoodgroveBankApiClient
{
    public async Task CreateAccount(Account account)
    {
        await RestService.For<IWoodgroveBankApiClient>(httpClient,
            new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer()
            }).CreateAccount(account);
    }

    public async Task CreateCustomer(Customer customer)
    {
        await RestService.For<IWoodgroveBankApiClient>(httpClient).CreateCustomer(customer);
    }

    public async Task<Customer[]> GetCustomers()
    {
        return await RestService.For<IWoodgroveBankApiClient>(httpClient).GetCustomers();
    }

    public async Task<Customer> SignIn(string customerPin)
    {
        return await RestService.For<IWoodgroveBankApiClient>(httpClient).SignIn(customerPin);
    }
}
