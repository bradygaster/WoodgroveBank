using WoodgroveBank.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.AddWoodgroveBankSilo(siloBuilder =>
{
    siloBuilder.ConfigureServices(services =>
    {
        services.AddHostedService<AdminDashboardObserverHost>();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

public class AdminDashboardObserver : IAdminDashboardObserver
{
    public void OnCustomerIndexUpdated(Customer customer)
    {
        Console.WriteLine($"{customer.Name} updated");
    }
}

public class AdminDashboardObserverHost : IHostedService
{
    private IAdminDashboardObserver _observer;
    private IClusterClient _clusterClient;
    private IBankGrain _bankGrain;
    private IAdminDashboardObserver _observerReference;
    private ILogger<AdminDashboardObserverHost> _logger;
    private Timer _delay;

    public AdminDashboardObserverHost(IClusterClient clusterClient, ILogger<AdminDashboardObserverHost> logger)
    {
        _observer = new AdminDashboardObserver();
        _clusterClient = clusterClient;
        _logger = logger;
    }

    private async Task Start()
    {
        _delay.Dispose();

        _logger.LogInformation("Getting bank grain from cluster");
        _bankGrain = _clusterClient.GetGrain<IBankGrain>(Guid.Empty);

        _logger.LogInformation("Getting customer list from grain to initialize it.");
        var customers = await _bankGrain.GetCustomers();
        _logger.LogInformation($"Got {customers.Length} customers");

        _logger.LogInformation("Creating reference to the local observer instance.");
        _observerReference = _clusterClient.CreateObjectReference<IAdminDashboardObserver>(_observer);
        _logger.LogInformation("Created reference to the local observer instance.");

        if (_observerReference == null)
        {
            _logger.LogInformation("Created reference IS NULL.");
        }
        else
        {
            _logger.LogInformation("Subscribing using the observer.");
            await _bankGrain.Subscribe(_observerReference);
            _logger.LogInformation("Subscribed using the observer.");
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _delay = new Timer(async _ => await Start(), null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _bankGrain.Unsubscribe(_observerReference);
    }
}
