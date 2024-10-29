public class TransactionSimulator(ILogger<TransactionSimulator> logger, IClusterClient clusterClient) : BackgroundService
{
    private BankSettings _bankSettings = new();
    private Customer[] _customers = Array.Empty<Customer>();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _bankSettings = await clusterClient.GetGrain<IBankGrain>(Guid.Empty).GetSettings();
                _customers = await clusterClient.GetGrain<IBankGrain>(Guid.Empty).GetCustomers();

                if (_customers.Length > 0)
                {
                    var randomCustomer = _customers[Random.Shared.Next(0, _customers.Length)];
                    var customer = clusterClient.GetGrain<ICustomerGrain>(randomCustomer.Id);
                    var accounts = await customer.GetAccounts();

                    if (accounts.Length > 0)
                    {
                        var account = accounts[Random.Shared.Next(0, accounts.Length)];
                        var accountGrain = clusterClient.GetGrain<IAccountGrain>(account.Id);
                        var balance = await accountGrain.GetBalance();
                        var amount = Random.Shared.Next(0, (int)account.Balance + 10);
                        var result = await accountGrain.Withdraw(amount);

                        if (result == false)
                        {
                            logger.LogWarning($"Withdrawal of {amount} from account {account.Id} failed");

                            amount = Random.Shared.Next(100, 1000);

                            logger.LogWarning($"Depositing {amount} into account {account.Id}");
                            var depositResult = await accountGrain.Deposit(amount);
                            if (depositResult)
                            {
                                logger.LogWarning($"Deposited {amount} into account {account.Id}");
                            }
                        }
                        else
                        {
                            logger.LogInformation($"Withdrawal of {amount} from account {account.Id} succeeded");
                        }
                    }
                }
            }
            catch (Exception)
            {
                // API or Bank silos aren't active yet
            }

            await Task.Delay(Random.Shared.Next(0, _bankSettings.MaximumDurationBetweenNewTransactions), stoppingToken);
        }
    }
}
