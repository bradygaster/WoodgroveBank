using Orleans.Streams;

namespace WoodgroveBank.Web.Events;

public class CustomerReceivedStreamHandler(IGrainFactory grainFactory,
        IClusterClient client) : IHostedService
{
    private readonly IStreamProvider _streamProvider = client.GetStreamProvider("BANK");
    public event EventHandler<Customer> CustomerReceived;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        grainFactory.GetGrain<IBankGrain>(Guid.Empty);

        var recentCustomerStreamId = StreamId.Create("BANK", "RECENT_CUSTOMERS");
        var recentCustomerStream = _streamProvider.GetStream<Customer>(recentCustomerStreamId);
        await recentCustomerStream.SubscribeAsync<Customer>(async (customer, token) =>
        {
            OnCustomerReceived(customer);
            await Task.CompletedTask;
        });
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public void OnCustomerReceived(Customer customer)
    {
        if (CustomerReceived != null)
            CustomerReceived(this, customer);
    }
}
