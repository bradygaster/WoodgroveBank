using Orleans.Concurrency;

namespace WoodgroveBank.Grains;

[Reentrant]
[CollectionAgeLimit(Minutes = 2)]
public class TransactionGrain(
    [PersistentState("transaction", "grainState")] IPersistentState<AccountTransaction> transactionState) 
        : Grain, IAccountTransactionGrain
{
    public Task<AccountTransaction> GetTransaction() => Task.FromResult(transactionState.State);

    public async Task SetTransaction(AccountTransaction transaction)
    {
        transactionState.State = transaction;
        await transactionState.WriteStateAsync();
    }

    public async Task Process()
    {
        await transactionState.ReadStateAsync();

        if (transactionState.State.TransactionType == TransactionType.Deposit)
        {
            transactionState.State.PotentialResultingAccountBalance = transactionState.State.InitialAccountBalance + transactionState.State.TransactionAmount;
            transactionState.State.ResultingAccountBalance = transactionState.State.InitialAccountBalance + transactionState.State.TransactionAmount;
            transactionState.State.TransactionAllowed = true;
        }

        if (transactionState.State.TransactionType == TransactionType.Withdrawal)
        {
            transactionState.State.PotentialResultingAccountBalance = transactionState.State.InitialAccountBalance - transactionState.State.TransactionAmount;

            if (transactionState.State.PotentialResultingAccountBalance > 0)
            {
                // account can cover - allow the transaction
                transactionState.State.TransactionAllowed = true;
                transactionState.State.ResultingAccountBalance = transactionState.State.PotentialResultingAccountBalance;
            }
            else
            {
                // account would overdraft - halt the withdrawal
                transactionState.State.TransactionAllowed = false;

                // now enable the overdraft penalty charge to their account
                transactionState.State.TransactionType = TransactionType.OverdraftPenalty;
            }
        }

        // each time they overdraft charge them $1
        if (transactionState.State.TransactionType == TransactionType.OverdraftPenalty)
        {
            // charge the customer $1 for overdrafting
            transactionState.State.ResultingAccountBalance = transactionState.State.ResultingAccountBalance - 1;

            // allow the mutated transaction to be saved
            transactionState.State.TransactionAllowed = true;
        }

        await SetTransaction(transactionState.State);
    }
}
