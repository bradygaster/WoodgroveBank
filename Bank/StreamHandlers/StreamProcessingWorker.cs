using Orleans.Streams;

namespace WoodgroveBank.Web.Events;

public class StreamProcessingWorker(IClusterClient client,
    TransactionProcessedStreamHandler transactionProcessedStreamHandler,
    CustomerReceivedStreamHandler customerReceivedStreamHandler) : IHostedService
{
    private readonly IStreamProvider streamProvider = client.GetStreamProvider("BANK");
    private StreamId recentCustomerStreamId = StreamId.Create("BANK", "RECENT_CUSTOMERS");
    private StreamId recentTransactionsStreamId = StreamId.Create("BANK", "RECENT_TRANSACTIONS");
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        client.GetGrain<IBankGrain>(Guid.Empty);
        var recentTransactionsStream = streamProvider.GetStream<Transaction>(recentTransactionsStreamId);
        var recentCustomerStream = streamProvider.GetStream<Customer>(recentCustomerStreamId);

        await recentTransactionsStream.SubscribeAsync<Transaction>(async (transaction, token) =>
        {
            transactionProcessedStreamHandler.OnTransactionReceived(new TransactionProcessedEventArgs
            {
                Transaction = transaction,
                Customer = await client.GetGrain<ICustomerGrain>(transaction.CustomerId).GetCustomer()
            });
        });

        await recentCustomerStream.SubscribeAsync<Customer>(async (customer, token) =>
        {
            customerReceivedStreamHandler.OnCustomerReceived(customer);
            await Task.CompletedTask;
        });
    }
}
