using Bogus;
using Refit;
using WoodgroveBank.Abstractions;

Console.WriteLine("Ready to connect to the Woodgrove Bank API.");
Console.WriteLine("Hit enter when server is up.");
Console.ReadLine();

var apiClient = new WoodgroveBankApi();

for (int i = 0; i < 3; i++)
{
    var customerId = Guid.NewGuid();
    var faker = new Faker<Customer>()
        .RuleFor(p => p.Id, f => customerId)
        .RuleFor(p => p.Name, f => f.Name.FullName())
        .RuleFor(p => p.Country, f => f.Address.Country())
        .RuleFor(p => p.City, f => $"{f.Address.City()}, {f.Address.State()}");

    var fakeCustomer = faker.Generate();

    await apiClient.CreateCustomer(fakeCustomer);

    await apiClient.CreateAccount(new Account
    {
        Type = AccountType.Checking,
        Name = "Checking",
        CustomerId = customerId,
        Balance = new Random().Next(1000, 1350),
        DateOfLastActivity = DateTime.Now,
        DateOpened = DateTime.Now,
        Id = Guid.NewGuid()
    });

    await apiClient.CreateAccount(new Account
    {
        Type = AccountType.Savings,
        Name = "Savings",
        CustomerId = customerId,
        Balance = new Random().Next(2000, 5000),
        DateOfLastActivity = DateTime.Now,
        DateOpened = DateTime.Now,
        Id = Guid.NewGuid()
    });
}

var customers = await apiClient.GetCustomers();

foreach (var customer in customers)
{
    Console.WriteLine($"Customer {customer.Name} lives in {customer.City} in {customer.Country}");
}

Console.WriteLine("Finished");
Console.ReadLine();

public class WoodgroveBankApi : IWoodgroveBankApi
{
    public async Task CreateAccount(Account account)
    {
        await RestService.For<IWoodgroveBankApi>("http://localhost:5001",
            new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer()
            }).CreateAccount(account);
    }

    public async Task CreateCustomer(Customer customer)
    {
        await RestService.For<IWoodgroveBankApi>(new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:5001")
        }).CreateCustomer(customer);
    }

    public async Task<Customer[]> GetCustomers()
    {
        return await RestService.For<IWoodgroveBankApi>(new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:5001")
        }).GetCustomers();
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
}