using Bogus;
using Refit;
using WoodgroveBank.Abstractions;

var createRandomCustomers = async () =>
{
    var apiClient = new WoodgroveBankApi();

    for (int i = 0; i < 3; i++)
    {
        Task.Delay(300);
        var customerId = Guid.NewGuid();
        var faker = new Faker<Customer>()
            .RuleFor(p => p.Id, f => customerId)
            .RuleFor(p => p.Name, f => f.Name.FullName())
            .RuleFor(p => p.Country, f => f.Address.Country())
            .RuleFor(p => p.City, f => $"{f.Address.City()}, {f.Address.State()}")
            .RuleFor(p => p.Pin, new Random().Next(1000, 9999).ToString());

        try
        {
            var fakeCustomer = faker.Generate();

            Console.WriteLine($"Creating customer {fakeCustomer.Name} in {fakeCustomer.City} in {fakeCustomer.Country}");
            await apiClient.CreateCustomer(fakeCustomer);

            var checking = new Account
            {
                Type = AccountType.Checking,
                Name = "Checking",
                CustomerId = customerId,
                Balance = new Random().Next(1000, 1350),
                DateOfLastActivity = DateTime.Now,
                DateOpened = DateTime.Now,
                Id = Guid.NewGuid()
            };

            var savings = new Account
            {
                Type = AccountType.Savings,
                Name = "Savings",
                CustomerId = customerId,
                Balance = new Random().Next(2000, 5000),
                DateOfLastActivity = DateTime.Now,
                DateOpened = DateTime.Now,
                Id = Guid.NewGuid()
            };

            Console.WriteLine($"Creating account {checking.Name} for customer {fakeCustomer.Name} with a balance of {checking.Balance}.");
            apiClient.CreateAccount(checking);

            Console.WriteLine($"Creating account {savings.Name} for customer {fakeCustomer.Name} with a balance of {savings.Balance}.");
            apiClient.CreateAccount(savings);
        }
        catch
        {
            // just keep going
        }
    }
};

var askToRepeat = () =>
{
    Console.WriteLine("Finished. Would you like me to repeat? (Y/n)");
    var repeat = Console.ReadLine();
    if (string.IsNullOrEmpty(repeat)) repeat = "y";
    return repeat;
};

Console.WriteLine("Ready to connect to the Woodgrove Bank API.");
Console.WriteLine("Hit enter when server is up.");
Console.ReadLine();

await createRandomCustomers();

var repeat = askToRepeat();
while (repeat.ToLower() == "y")
{
    await createRandomCustomers();
    repeat = askToRepeat();
}

public class WoodgroveBankApi : IWoodgroveBankApi
{
    const string URI = "http://localhost:31000";
    HttpClient _httpClient;

    public WoodgroveBankApi()
    {
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri(URI)
        };
    }

    public async Task CreateAccount(Account account)
    {
        await RestService.For<IWoodgroveBankApi>(_httpClient,
            new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer()
            }).CreateAccount(account);
    }

    public async Task CreateCustomer(Customer customer)
    {
        await RestService.For<IWoodgroveBankApi>(_httpClient).CreateCustomer(customer);
    }

    public async Task<Customer[]> GetCustomers()
    {
        return await RestService.For<IWoodgroveBankApi>(_httpClient).GetCustomers();
    }

    public async Task<Customer> SignIn(string customerPin)
    {
        return await RestService.For<IWoodgroveBankApi>(_httpClient).SignIn(customerPin);
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