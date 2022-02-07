using Orleans;
using Orleans.Runtime;
using WoodgroveBank.Abstractions;
using WoodgroveBank.Infrastructure;

namespace WoodgroveBank.Grains
{
    public class AccountGrain : Grain, IAccountGrain
    {
        public AccountGrain([PersistentState("account", Strings.OrleansPersistenceNames.AccountStore)] IPersistentState<Account> accountState,
            [PersistentState("accountTransactions", Strings.OrleansPersistenceNames.AccountTransactionsStore)] IPersistentState<List<Transaction>> transactionListState)
        {
            _accountState = accountState;
            _transactionListState = transactionListState;
        }

        private IPersistentState<Account> _accountState { get; set; }
        private IPersistentState<List<Transaction>> _transactionListState { get; set; }

        public async Task<Account> SaveAccount(Account account)
        {
            _accountState.State = account;
            await _accountState.WriteStateAsync();
            return _accountState.State;
        }

        public async Task<bool> Deposit(decimal amount)
        {
            return await SubmitTransation(TransactionType.Deposit, amount);
        }

        public async Task<bool> Withdraw(decimal amount)
        {
            return await SubmitTransation(TransactionType.Withdrawal, amount);
        }

        private async Task<bool> SubmitTransation(TransactionType type, decimal amount)
        {
            var transaction = new Transaction
            {
                AccountId = this.GetGrainIdentity().PrimaryKey,
                CustomerId = _accountState.State.CustomerId,
                InitialAccountBalance = _accountState.State.Balance,
                Timestamp = DateTime.Now,
                TransactionType = type,
                TransactionAmount = amount
            };

            transaction.TransactionAllowed = await GrainFactory.GetGrain<IBankGrain>(Guid.Empty).SubmitTransaction(transaction);
            _transactionListState.State.Add(transaction);
            await _transactionListState.WriteStateAsync();
            return transaction.TransactionAllowed;
        }
    }
}
