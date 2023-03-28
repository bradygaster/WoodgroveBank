using System.Text.Json.Serialization;

namespace WoodgroveBank.Abstractions
{
    [GenerateSerializer]
    public class Account
    {
        [Id(0)]
        public Guid Id { get; set; } = Guid.Empty;
        [Id(1)]
        public string Name { get; set; } = string.Empty;
        [Id(2)]
        public Guid CustomerId { get; set; }
        [Id(3)]
        public decimal Balance { get; set; }
        [Id(4)]
        public DateTime DateOpened { get; set; } = DateTime.MinValue;
        [Id(5)]
        public DateTime DateOfLastActivity { get; set; } = DateTime.MinValue;
        [Id(6)]
        public AccountStatus Status { get; set; } = AccountStatus.InGoodStanding;
        [Id(7)]
        public AccountType Type { get; set; } = AccountType.Checking;
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AccountType
    {
        Checking = 0,
        Savings = 1
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AccountStatus
    {
        InGoodStanding = 0,
        IsUnderObservation = 1
    }
}
