using Simulations;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AsOrleansClient();
builder.Services.AddHttpClient<WoodgroveBankApiClient>(client =>
{
    client.BaseAddress = new("https+http://api");
});
builder.Services.AddHostedService<TransactionSimulator>();
builder.Services.AddHostedService<NewCustomerSimulator>();
var app = builder.Build();

app.MapDefaultEndpoints();
app.Run();