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
.WithTags("Customers")
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
.WithTags("Customers")
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
.WithTags("Accounts")
.WithName("GetCustomerAccounts")
.Produces<List<Account>>();

/// <summary>
/// Create a new account for a customer.
/// </summary>
app.MapPost("/accounts", async (IGrainFactory grainFactory, Account account) =>
{
    try
    {
        var customerGrain = grainFactory.GetGrain<ICustomerGrain>(account.CustomerId);
        account = await customerGrain.OpenAccount(account);
        return Results.Created($"/accounts/{account.Id}", account);
    }
    catch (Exception ex)
    {
        return Results.Conflict(ex.Message);
    }
})
.WithTags("Accounts")
.WithName("CreateAccount")
.Produces(StatusCodes.Status409Conflict)
.Produces<Account>(StatusCodes.Status201Created);

/// <summary>
/// Submits a deposit to a customer account.
/// </summary>
app.MapPost("/accounts/{accountId}/deposit/", 
    async (IGrainFactory grainFactory, Guid accountId, decimal amount) =>
{
    var result = await grainFactory.GetGrain<IAccountGrain>(accountId).Deposit(amount);
    if (result) return Results.Ok();
    return Results.Unauthorized();
})
.WithTags("Accounts")
.WithName("SubmitDeposit")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized);

/// <summary>
/// Submits a withdrawal to a customer account.
/// </summary>
app.MapPost("/accounts/{accountId}/withdraw", 
    async (IGrainFactory grainFactory, Guid accountId, decimal amount) =>
{
    var result = await grainFactory.GetGrain<IAccountGrain>(accountId).Withdraw(amount);
    if(result) return Results.Ok();
    return Results.Unauthorized();
})
.WithTags("Accounts")
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
.WithTags("Accounts")
.WithName("GetTransactions")
.Produces<List<Transaction>>();

/// <summary>
/// Signs a customer in using their ID.
/// </summary>
app.MapGet("/atm/signin/{customerPin}", async (IGrainFactory grainFactory, string customerPin) =>
{
    var customer = await grainFactory.GetGrain<IBankGrain>(Guid.Empty).AuthenticateCustomer(customerPin);
    if(customer == null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(customer);
})
.WithTags("ATM")
.WithName("SignIn")
.Produces<Customer>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized);

// run the api
app.Run();
