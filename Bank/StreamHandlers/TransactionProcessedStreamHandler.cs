namespace WoodgroveBank.Web.Events;

public class TransactionProcessedStreamHandler
{
    public event EventHandler<TransactionProcessedEventArgs>? TransactionReceived;

    public void OnTransactionReceived(TransactionProcessedEventArgs args)
    {
        if (TransactionReceived != null)
            TransactionReceived(this, args);
    }
}

public class TransactionProcessedEventArgs : EventArgs
{
    public required AccountTransaction Transaction { get; set; }
    public required Customer Customer { get; set; }
}