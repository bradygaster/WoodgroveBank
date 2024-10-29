namespace WoodgroveBank.Abstractions;

[GenerateSerializer]
public class BankSettings
{
    [Id(0)]
    public int MaximumDurationBetweenNewCustomers { get; set; } = 10000;
    [Id(1)]
    public int MaximumDurationBetweenNewTransactions { get; set; } = 10000;
}
