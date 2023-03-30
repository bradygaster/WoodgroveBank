namespace WoodgroveBank.Abstractions
{
    public interface IAdminDashboardObserver : IGrainObserver
    {
        void OnCustomerIndexUpdated(Customer customer);
    }
}
