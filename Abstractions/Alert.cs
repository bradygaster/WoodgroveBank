namespace WoodgroveBank.Abstractions
{
    [GenerateSerializer]
    public class Alert
    {
        [Id(0)]
        public int Id { get; set; }
        [Id(1)]
        public DateTime DateOfAlert { get; set; } = DateTime.Now;
        [Id(2)]
        public Alerts AlertType { get; set; } = Alerts.AttemptedHack;
    }

    public enum Alerts
    {
        AttemptedHack = 0,
        HugeDeposit = 1
    }
}
