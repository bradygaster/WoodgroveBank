using Orleans.Concurrency;
using Orleans.Runtime;
using WoodgroveBank.Abstractions;

namespace WoodgroveBank.Grains
{
    [Reentrant]
    public class AccountGrain : Grain, IAccountGrain
    {
        public AccountGrain([PersistentState("account")] IPersistentState<Account> accountState,
            [PersistentState("accountTransactions")] IPersistentState<List<Transaction>> transactionListState)
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
                AccountId = this.GetGrainId().GetGuidKey(),
                CustomerId = _accountState.State.CustomerId,
                InitialAccountBalance = _accountState.State.Balance,
                Timestamp = DateTime.Now,
                TransactionType = type,
                TransactionAmount = amount
            };

            transaction.TransactionAllowed = await SubmitTransaction(transaction);
            return transaction.TransactionAllowed;
        }

        private async Task<bool> SubmitTransaction(Transaction transaction)
        {
            await _accountState.ReadStateAsync();

            transaction.InitialAccountBalance = _accountState.State.Balance;

            if (transaction.TransactionType == TransactionType.Deposit)
            {
                transaction.PotentialResultingAccountBalance = (transaction.InitialAccountBalance + transaction.TransactionAmount);
                transaction.ResultingAccountBalance = (transaction.InitialAccountBalance + transaction.TransactionAmount);
                transaction.TransactionAllowed = true;
                _transactionListState.State.Add(transaction);
                _accountState.State.Balance = transaction.ResultingAccountBalance;
            }

            if (transaction.TransactionType == TransactionType.Withdrawal)
            {
                decimal withdrawalFee = (decimal)2.5;
                transaction.PotentialResultingAccountBalance = (transaction.InitialAccountBalance - (transaction.TransactionAmount + withdrawalFee));

                if (transaction.PotentialResultingAccountBalance > 0)
                {
                    // account can cover - allow the transaction
                    transaction.TransactionAllowed = true;
                    transaction.ResultingAccountBalance = transaction.PotentialResultingAccountBalance;
                    _accountState.State.Balance = transaction.ResultingAccountBalance;
                }
                else
                {
                    // account would overdraft - halt the transaction
                    transaction.TransactionAllowed = false;

                    // now enable the overdraft penalty charge to their account
                    transaction.TransactionType = TransactionType.OverdraftPenalty;
                }
            }

            // each time they overdraft charge them 1% and flag the account
            if (transaction.TransactionType == TransactionType.OverdraftPenalty)
            {
                transaction.TransactionAllowed = true;
                _accountState.State.Balance = (transaction.InitialAccountBalance * (decimal).99);
            }

            transaction.Timestamp = DateTime.Now;
            _transactionListState.State.Add(transaction);
            await _transactionListState.WriteStateAsync();
            await _accountState.WriteStateAsync();

            await GrainFactory.GetGrain<ICustomerGrain>(transaction.CustomerId).ReceiveAccountUpdate(_accountState.State);

            return transaction.TransactionAllowed && transaction.ResultingAccountBalance > 0;
        }

        public async Task<Transaction[]> GetTransactions()
        {
            await _accountState.ReadStateAsync();
            return _transactionListState.State.ToArray();
        }

        public Task<decimal> GetBalance() => Task.FromResult(_accountState.State.Balance);
    }
}
