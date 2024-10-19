using Bogus;

namespace Simulations;

public class NewCustomerSimulator(WoodgroveBankApiClient woodgroveBankApiClient) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
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
                await woodgroveBankApiClient.CreateCustomer(fakeCustomer);

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
                woodgroveBankApiClient.CreateAccount(checking);

                Console.WriteLine($"Creating account {savings.Name} for customer {fakeCustomer.Name} with a balance of {savings.Balance}.");
                woodgroveBankApiClient.CreateAccount(savings);
            }
            catch
            {
                // just keep going
            }

            await Task.Delay(Random.Shared.Next(3000, 5000), stoppingToken);
        }
    }
}
