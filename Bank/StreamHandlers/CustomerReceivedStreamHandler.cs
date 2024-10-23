namespace WoodgroveBank.Web.Events;

public class CustomerReceivedStreamHandler
{
    public event EventHandler<Customer>? CustomerReceived;

    public void OnCustomerReceived(Customer customer)
    {
        if (CustomerReceived != null)
            CustomerReceived(this, customer);
    }
}
