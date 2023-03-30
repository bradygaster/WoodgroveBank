namespace WoodgroveBank.Abstractions
{
    public interface IAdminDashboardObserver : IGrainObserver
    {
        Task OnCustomerIndexUpdated(Customer customer);
    }
}
