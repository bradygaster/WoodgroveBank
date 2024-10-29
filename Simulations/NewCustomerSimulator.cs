using Bogus;
using WoodgroveBank.Abstractions;

namespace Simulations;

public class NewCustomerSimulator(ILogger<NewCustomerSimulator> logger, IClusterClient clusterClient) : BackgroundService
{
    private BankSettings _bankSettings = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _bankSettings = await clusterClient.GetGrain<IBankGrain>(Guid.Empty).GetSettings();

                var customerId = Guid.NewGuid();
                var faker = new Faker<Customer>()
                    .RuleFor(p => p.Id, f => customerId)
                    .RuleFor(p => p.Name, f => f.Name.FullName())
                    .RuleFor(p => p.Country, f => f.Address.Country())
                    .RuleFor(p => p.City, f => $"{f.Address.City()}, {f.Address.State()}")
                    .RuleFor(p => p.Pin, new Random().Next(1000, 9999).ToString());

                var fakeCustomer = faker.Generate();

                logger.LogInformation($"Creating customer {fakeCustomer.Name} in {fakeCustomer.City} in {fakeCustomer.Country}");
                await clusterClient.GetGrain<ICustomerGrain>(customerId).SaveCustomer(fakeCustomer);
                await clusterClient.GetGrain<IBankGrain>(Guid.Empty).UpdateCustomerIndex(fakeCustomer);

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

                logger.LogInformation($"Creating account {checking.Name} for customer {fakeCustomer.Name} with a balance of {checking.Balance}.");
                await clusterClient.GetGrain<ICustomerGrain>(customerId).OpenAccount(checking);

                logger.LogInformation($"Creating account {savings.Name} for customer {fakeCustomer.Name} with a balance of {savings.Balance}.");
                await clusterClient.GetGrain<ICustomerGrain>(customerId).OpenAccount(savings);
            }
            catch
            {
                // just keep going
            }

            await Task.Delay(Random.Shared.Next(0, _bankSettings.MaximumDurationBetweenNewCustomers), stoppingToken);
        }
    }
}
