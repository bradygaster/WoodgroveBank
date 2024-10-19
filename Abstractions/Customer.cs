namespace WoodgroveBank.Abstractions
{
    [GenerateSerializer]
    public class Customer
    {
        [Id(0)]
        public Guid Id { get; set; }
        [Id(1)]
        public string Name { get; set; } = string.Empty;
        [Id(2)]
        public string City { get; set; } = string.Empty;
        [Id(3)]
        public string Country { get; set; } = string.Empty;
        [Id(4)]
        public string Pin { get; set; } = string.Empty;
    }
}
