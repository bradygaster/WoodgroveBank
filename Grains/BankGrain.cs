using Orleans;
using Orleans.Runtime;
using WoodgroveBank.Abstractions;
using WoodgroveBank.Infrastructure;

namespace WoodgroveBank.Grains
{
    public class BankGrain : Grain, IBankGrain
    {
        private IPersistentState<List<Transaction>> _transactionListState { get; set; }
        private IPersistentState<List<Account>> _accountListState { get; set; }

        public BankGrain([PersistentState("transactions", Strings.OrleansPersistenceNames.TransactionsStore)] IPersistentState<List<Transaction>> transactionListState,
            [PersistentState("accounts", Strings.OrleansPersistenceNames.AccountsStore)] IPersistentState<List<Account>> accountListState)
        {
            _transactionListState = transactionListState;
            _accountListState = accountListState;
        }

        public Task<Account> OpenAccount(string name, Customer customer, AccountType accountType, decimal amount)
        {
            throw new NotImplementedException();
        }

        public Task<Account[]> GetAccounts()
        {
            return Task.FromResult(_accountListState.State.OrderBy(x => x.DateOfLastActivity).ToArray());
        }

        public Task<Transaction[]> GetRecentTransactions()
        {
            return Task.FromResult(_transactionListState.State.OrderBy(x => x.Timestamp).ToArray());
        }

        public async Task<bool> SubmitTransaction(Transaction transaction)
        {
            transaction.InitialAccountBalance = _accountListState.State.First(x => x.Id == transaction.AccountId).Balance;

            if (transaction.TransactionType == TransactionType.Deposit)
            {
                transaction.PotentialResultingAccountBalance = (transaction.InitialAccountBalance + transaction.TransactionAmount);
                transaction.ResultingAccountBalance = (transaction.InitialAccountBalance + transaction.TransactionAmount);
                transaction.TransactionAllowed = true;
                _transactionListState.State.Add(transaction);
                _accountListState.State.First(x => x.Id == transaction.AccountId).Balance = transaction.ResultingAccountBalance;
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
                    _accountListState.State.First(x => x.Id == transaction.AccountId).Balance = transaction.ResultingAccountBalance;
                }
                else
                {
                    // account would overdraft - halt the transaction
                    transaction.TransactionAllowed = false;
                }
            }

            // each time they overdraft charge them 1% and flag the account
            if (transaction.TransactionType == TransactionType.OverdraftPenalty) 
            {
                transaction.TransactionAllowed = true;
                _accountListState.State.First(x => x.Id == transaction.AccountId).Balance = (transaction.InitialAccountBalance * (decimal).99);
            }

            transaction.Timestamp = DateTime.Now;
            _transactionListState.State.Add(transaction);
            await _transactionListState.WriteStateAsync();
            await _accountListState.WriteStateAsync();

            return transaction.TransactionAllowed;
        }
    }
}
