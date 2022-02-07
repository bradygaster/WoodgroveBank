using Orleans;
using WoodgroveBank.Abstractions;
using WoodgroveBank.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddWoodvilleBankSilo();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

/// <summary>
/// Get a list of all the customers.
/// </summary>
app.MapGet("/customers", async (IGrainFactory grainFactory) =>
    await grainFactory.GetGrain<IBankGrain>(Guid.Empty).GetCustomers()
)
.WithName("GetCustomers");

/// <summary>
/// Create a new customer.
/// </summary>
app.MapPost("/customers", async (IGrainFactory grainFactory, Customer customer) =>
{
    try
    {
        var result = await grainFactory.GetGrain<ICustomerGrain>(customer.Id).SaveCustomer(customer);
        await grainFactory.GetGrain<IBankGrain>(Guid.Empty).UpdateCustomerIndex(customer);
        return Results.Created($"/customers/{result.Id}", result);
    }
    catch (Exception ex)
    {
        return Results.Conflict(ex.Message);
    }
})
.WithName("CreateCustomer")
.Produces(StatusCodes.Status409Conflict)
.Produces<Customer>(StatusCodes.Status201Created);

/// <summary>
/// Create a new account for a customer.
/// </summary>
app.MapPost("/customers/{id}/accounts", async (IGrainFactory grainFactory, Guid id, Account account) =>
{
    try
    {
        var customerGrain = grainFactory.GetGrain<ICustomerGrain>(id);
        account = await customerGrain.OpenAccount(account);

        return Results.Created($"/accounts/{account.Id}", account);
    }
    catch (Exception ex)
    {
        return Results.Conflict(ex.Message);
    }
})
.WithName("CreateAccount")
.Produces(StatusCodes.Status409Conflict)
.Produces<Account>(StatusCodes.Status201Created);

/// <summary>
/// Gets all of a customer's accounts.
/// </summary>
app.MapGet("/customers/{id}/accounts", async (IGrainFactory grainFactory, Guid id) =>
{
    var customerGrain = grainFactory.GetGrain<ICustomerGrain>(id);
    return Results.Ok(await customerGrain.GetAccounts());
})
.WithName("GetCustomerAccounts")
.Produces<List<Account>>();

/// <summary>
/// Gets all of a customer's transactions for an account.
/// </summary>
app.MapGet("/customers/{customerId}/accounts/{accountId}/transactions", (IGrainFactory grainFactory, string customerId, string accountId) =>
{

})
.WithName("GetCustomerAccountTransactions")
.Produces<List<Account>>();

// run the api
app.Run();
