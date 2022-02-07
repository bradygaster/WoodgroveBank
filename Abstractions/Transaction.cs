namespace WoodgroveBank.Abstractions
{
    public class Transaction
    {
        public Guid AccountId { get; set; }
        public Guid CustomerId { get; set; }
        public TransactionType TransactionType { get; set; }
        public decimal TransactionAmount { get; set; }
        public decimal InitialAccountBalance { get; set; }
        public decimal ResultingAccountBalance { get; set; }
        public decimal PotentialResultingAccountBalance { get; set; }
        public bool TransactionAllowed { get; set; } = false;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public enum TransactionType
    {
        Deposit = 0,
        Withdrawal = 1,
        OverdraftPenalty = 2
    }
}
