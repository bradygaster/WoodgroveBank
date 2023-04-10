using Orleans.Runtime;
using Orleans.Streams;
using WoodgroveBank.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.AddWoodgroveBankSilo();
builder.Services.AddSingleton<CustomerReceivedProxy>();
builder.Services.AddHostedService<CustomerStreamHost>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

public class CustomerStreamHost : IHostedService
{
    private readonly IClusterClient _client;
    private readonly CustomerReceivedProxy _proxy;
    private readonly IStreamProvider _streamProvider;
    private readonly ILogger<CustomerStreamHost> _logger;

    public CustomerStreamHost(IClusterClient client,
        CustomerReceivedProxy proxy,
        ILogger<CustomerStreamHost> logger)
    {
        _client = client;
        _proxy = proxy;
        _logger = logger;
        _streamProvider = client.GetStreamProvider("ADMIN");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var recentCustomerStreamId = StreamId.Create("ADMIN", "RECENT_CUSTOMERS");
        var stream = _streamProvider.GetStream<Customer>(recentCustomerStreamId);
        await stream.SubscribeAsync<Customer>(async (customer, token) =>
        {
            _logger.LogInformation($"New customer: {customer.Name} {customer.Id}");
            _proxy.OnCustomerReceived(customer);
            await Task.CompletedTask;
        });
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

public class CustomerReceivedProxy
{
    public event EventHandler<Customer> CustomerReceived;

    public void OnCustomerReceived(Customer customer)
    {
        if (CustomerReceived != null)
            CustomerReceived(this, customer);
    }
}