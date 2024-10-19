using Orleans.Streams;

namespace WoodgroveBank.Web.Events;

public class TransactionProcessedStreamHandler(IGrainFactory grainFactory,
        IClusterClient client) : IHostedService
{
    private readonly IStreamProvider _streamProvider = client.GetStreamProvider("BANK");
    public event EventHandler<TransactionProcessedEventArgs>? TransactionReceived;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        grainFactory.GetGrain<IBankGrain>(Guid.Empty);
        
        var recentTransactionsStreamId = StreamId.Create("BANK", "RECENT_TRANSACTIONS");
        var recentTransactionsStream = _streamProvider.GetStream<Transaction>(recentTransactionsStreamId);
        await recentTransactionsStream.SubscribeAsync<Transaction>(async (transaction, token) =>
        {
            OnTransactionReceived(new TransactionProcessedEventArgs
            {
                Transaction = transaction,
                Customer = await grainFactory.GetGrain<ICustomerGrain>(transaction.CustomerId).GetCustomer()
            });

            await Task.CompletedTask;
        });
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public void OnTransactionReceived(TransactionProcessedEventArgs args)
    {
        if (TransactionReceived != null)
            TransactionReceived(this, args);
    }
}

public class TransactionProcessedEventArgs : EventArgs
{
    public required Transaction Transaction { get; set; }
    public required Customer Customer { get; set; }
}