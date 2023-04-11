using WoodgroveBank.Abstractions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.AddWoodgroveBankSilo();
builder.Services.AddHostedService<Simulator>();
var app = builder.Build();
app.Run();

public class Simulator : BackgroundService
{
    private readonly ILogger<Simulator> _logger;
    private IClusterClient _clusterClient;

    public Simulator(ILogger<Simulator> logger, IClusterClient clusterClient)
    {
        _logger = logger;
        _clusterClient = clusterClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // get the customers
            var bank = _clusterClient.GetGrain<IBankGrain>(Guid.Empty);
            var customers = await bank.GetCustomers();

            if (customers.Length > 0)
            {
                var randomCustomer = customers[Random.Shared.Next(0, customers.Length)];
                var customer = _clusterClient.GetGrain<ICustomerGrain>(randomCustomer.Id);
                var accounts = await customer.GetAccounts();

                if (accounts.Length > 0)
                {
                    var account = accounts[Random.Shared.Next(0, accounts.Length)];
                    var accountGrain = _clusterClient.GetGrain<IAccountGrain>(account.Id);
                    var balance = await accountGrain.GetBalance();
                    var amount = Random.Shared.Next(0, (int)account.Balance + 10);
                    var result = await accountGrain.Withdraw(amount);
                    
                    if (result == false)
                    {
                        _logger.LogError($"Withdrawal of {amount} from account {account.Id} failed");

                        amount = Random.Shared.Next(100, 1000);

                        _logger.LogWarning($"Depositing {amount} into account {account.Id}");
                        var depositResult = await accountGrain.Deposit(amount);
                        if(depositResult)
                        {
                            _logger.LogWarning($"Deposited {amount} into account {account.Id}");
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"Withdrawal of {amount} from account {account.Id} succeeded");
                    }
                }
            }

            await Task.Delay(Random.Shared.Next(100, 2000), stoppingToken);
        }
    }
}