namespace WoodgroveBank.Abstractions
{
    [GenerateSerializer]
    public class Customer
    {
        [Id(0)]
        public Guid Id { get; set; }
        [Id(1)]
        public string Name { get; set; }
        [Id(2)]
        public string City { get; set; }
        [Id(3)]
        public string Country { get; set; }
        [Id(4)]
        public string Pin { get; set; }
    }
}
