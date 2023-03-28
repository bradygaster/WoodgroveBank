namespace WoodgroveBank.Abstractions
{
    [GenerateSerializer]
    public class Transaction
    {
        [Id(0)]
        public Guid AccountId { get; set; }
        [Id(1)]
        public Guid CustomerId { get; set; }
        [Id(2)]
        public TransactionType TransactionType { get; set; }
        [Id(3)]
        public decimal TransactionAmount { get; set; }
        [Id(4)]
        public decimal InitialAccountBalance { get; set; }
        [Id(5)]
        public decimal ResultingAccountBalance { get; set; }
        [Id(6)]
        public decimal PotentialResultingAccountBalance { get; set; }
        [Id(7)]
        public bool TransactionAllowed { get; set; } = false;
        [Id(8)]
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public enum TransactionType
    {
        Deposit = 0,
        Withdrawal = 1,
        OverdraftPenalty = 2
    }
}
