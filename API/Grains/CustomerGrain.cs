namespace API.Grains;

[CollectionAgeLimit(Minutes = 2)]
public class CustomerGrain : Grain, ICustomerGrain
{
    private IPersistentState<Customer> _customerState;
    private IPersistentState<List<Account>> _customerAccounts;
    private readonly ILogger<CustomerGrain> _logger;

    public CustomerGrain([PersistentState("customer", "grainState")] IPersistentState<Customer> customerState,
        [PersistentState("customerAccounts", "grainState")] IPersistentState<List<Account>> customerAccounts,
        ILogger<CustomerGrain> logger)
    {
        _customerState = customerState;
        _customerAccounts = customerAccounts;
        _logger = logger;
    }

    public async Task<Customer> SaveCustomer(Customer customer)
    {
        _customerState.State = customer;
        await _customerState.WriteStateAsync();
        await _customerAccounts.WriteStateAsync();
        return _customerState.State;
    }

    public async Task<Customer> GetCustomer()
    {
        await _customerState.ReadStateAsync();
        await _customerAccounts.ReadStateAsync();
        return _customerState.State;
    }

    public async Task<Account> OpenAccount(Account account)
    {
        if (account.Id == Guid.Empty)
        {
            account.Id = Guid.NewGuid();
        }

        try
        {
            account.CustomerId = this.GetGrainId().GetGuidKey();
            var accountGrain = GrainFactory.GetGrain<IAccountGrain>(account.Id);
            account = await accountGrain.SaveAccount(account);

            await ReceiveAccountUpdate(account);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }


        return account;
    }

    public async Task<Account[]> GetAccounts()
    {
        await _customerAccounts.ReadStateAsync();
        return _customerAccounts.State.ToArray();
    }

    public async Task ReceiveAccountUpdate(Account account)
    {
        if (!_customerAccounts.State.Any(x => x.Id == account.Id))
        {
            _customerAccounts.State.Add(account);
        }
        else
        {
            _customerAccounts.State.First(x => x.Id == account.Id).Balance = account.Balance;
        }

        await _customerAccounts.WriteStateAsync();
    }
}
