using WoodgroveBank.Abstractions;

namespace Admin
{
    public class AdminDashboardObserver : IAdminDashboardObserver
    {
        private ILogger<AdminDashboardObserver> _logger;

        public AdminDashboardObserver()
        {
        }

        public AdminDashboardObserver(ILogger<AdminDashboardObserver> logger)
        {
            _logger = logger;
        }

        public void OnCustomerIndexUpdated(Customer customer)
        {
            _logger.LogInformation($"{customer.Name} updated");
        }
    }
}
