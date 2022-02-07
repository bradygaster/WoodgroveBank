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
/// Submits a deposit to a customer account.
/// </summary>
app.MapPost("/customers/{customerId}/accounts/{accountId}/deposit/", 
    async (IGrainFactory grainFactory, Guid customerId, Guid accountId, decimal amount) =>
{
    var result = await grainFactory.GetGrain<IAccountGrain>(accountId).Deposit(amount);
    if (result) return Results.Ok();
    return Results.Unauthorized();
})
.WithName("SubmitDeposit")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized);

/// <summary>
/// Submits a withdrawal to a customer account.
/// </summary>
app.MapPost("/customers/{customerId}/accounts/{accountId}/withdraw", 
    async (IGrainFactory grainFactory, Guid customerId, Guid accountId, decimal amount) =>
{
    var result = await grainFactory.GetGrain<IAccountGrain>(accountId).Withdraw(amount);
    if(result) return Results.Ok();
    return Results.Unauthorized();
})
.WithName("SubmitWithdrawal")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized);

/// <summary>
/// Gets all of a customer's transactions for an account.
/// </summary>
app.MapGet("/accounts/{accountId}/transactions", async (IGrainFactory grainFactory, Guid accountId) =>
{
    var result = await grainFactory.GetGrain<IAccountGrain>(accountId).GetTransactions();
    return Results.Ok(result);
})
.WithName("GetTransactions")
.Produces<List<Transaction>>();

// run the api
app.Run();
