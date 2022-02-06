namespace WoodgroveBank.Abstractions
{
    public class Account
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public decimal Balance { get; set; }
        public DateTime DateOpened { get; set; } = DateTime.MinValue;
        public DateTime DateOfLastActivity { get; set; } = DateTime.MinValue;
        public AccountStatus Status { get; set; } = AccountStatus.InGoodStanding;
        public AccountType Type { get; set; } = AccountType.Checking;
    }

    public enum AccountType
    {
        Checking = 0,
        Savings = 1
    }

    public enum AccountStatus
    {
        InGoodStanding = 0,
        IsUnderObservation = 1
    }
}
