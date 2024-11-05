using Orleans.Concurrency;

namespace WoodgroveBank.Grains;

[Reentrant]
[CollectionAgeLimit(Minutes = 2)]

public class AccountGrain([PersistentState("account", "grainState")] IPersistentState<Account> accountState,
        [PersistentState("transactions", "grainState")] IPersistentState<List<AccountTransaction>> transactionListState) : Grain, IAccountGrain
{
    public Task<decimal> GetBalance() => Task.FromResult(accountState.State.Balance);

    public async Task<AccountTransaction[]> GetTransactions()
    {
        await transactionListState.ReadStateAsync();
        return transactionListState.State.ToArray();
    }

    public async Task<Account> SaveAccount(Account account)
    {
        accountState.State = account;
        await accountState.WriteStateAsync();
        return accountState.State;
    }

    public async Task<bool> Deposit(decimal amount)
        => await SubmitTransaction(TransactionType.Deposit, amount);

    public async Task<bool> Withdraw(decimal amount)
        => await SubmitTransaction(TransactionType.Withdrawal, amount);

    private async Task<bool> SubmitTransaction(TransactionType type, decimal amount)
    {
        var transaction = new AccountTransaction
        {
            AccountId = this.GetGrainId().GetGuidKey(),
            CustomerId = accountState.State.CustomerId,
            InitialAccountBalance = accountState.State.Balance,
            Timestamp = DateTime.Now,
            TransactionType = type,
            TransactionAmount = amount,
            TransactionId = Guid.NewGuid()
        };

        var transactionGrain = GrainFactory.GetGrain<IAccountTransactionGrain>(transaction.TransactionId);
        await transactionGrain.SetTransaction(transaction);
        await transactionGrain.Process();

        transaction = await transactionGrain.GetTransaction();
        transaction.Timestamp = DateTime.Now;

        transactionListState.State.Add(transaction);
        accountState.State.Balance = transaction.ResultingAccountBalance;

        var bank = GrainFactory.GetGrain<IBankGrain>(Guid.Empty);
        await bank.LogTransaction(transaction);

        await GrainFactory.GetGrain<ICustomerGrain>(transaction.CustomerId)
                            .ReceiveAccountUpdate(accountState.State);

        return transaction.TransactionAllowed;
    }
}
