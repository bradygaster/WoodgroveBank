namespace WoodgroveBank.Abstractions
{
    public class Alert
    {
        public int Id { get; set; }
        public DateTime DateOfAlert { get; set; } = DateTime.Now;
        public Alerts AlertType { get; set; } = Alerts.AttemptedHack;
    }

    public enum Alerts
    {
        AttemptedHack = 0,
        HugeDeposit = 1
    }
}
