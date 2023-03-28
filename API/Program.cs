using Microsoft.AspNetCore.Mvc;
using Orleans.Runtime;
using WoodgroveBank.Abstractions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddWoodgroveBankSilo();

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();

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

/// <summary>
/// Get high-level details about the Orleans cluster.
/// </summary>
app.MapGet("/system/grains", async ([FromServices] IGrainFactory grainFactory) =>
{
    var managementGrain = grainFactory.GetGrain<IManagementGrain>(0);
    var statistics = await managementGrain.GetDetailedGrainStatistics();
    var detailedHosts = await managementGrain.GetDetailedHosts();
    var silos = detailedHosts.Select(_ => new SiloInfo(_.SiloName, _.SiloAddress.ToGatewayUri().AbsoluteUri)).Distinct();
    var result = statistics.Select(_ => new GrainInfo(_.GrainType, _.GrainId.ToString(), silos.First(silo => silo.SiloAddress == _.SiloAddress.ToGatewayUri().AbsoluteUri).SiloName));
    return Results.Ok(result);
})
.WithTags("System")
.WithName("GrainDetails")
.Produces<GrainInfo[]>(StatusCodes.Status200OK);

/// <summary>
/// Get high-level details about the Orleans silos.
/// </summary>
app.MapGet("/system/silos", async ([FromServices] IGrainFactory grainFactory) =>
{
    var managementGrain = grainFactory.GetGrain<IManagementGrain>(0);
    var statistics = await managementGrain.GetDetailedGrainStatistics();
    var detailedHosts = await managementGrain.GetDetailedHosts();
    var silos = detailedHosts
                    .Where(x => x.Status == SiloStatus.Active)
                    .Select(_ => new SiloInfo(_.SiloName, _.SiloAddress.ToGatewayUri().AbsoluteUri));

    silos = silos.Distinct();
    return Results.Ok(silos);
})
.WithTags("System")
.WithName("SiloDetails")
.Produces<SiloInfo[]>(StatusCodes.Status200OK);

// run the api
app.Run();

// This record is used to display information about Grains hosted in the Orleans cluster.
public record GrainInfo(string Type, string PrimaryKey, string SiloAddress);

// This record is used to display information about silos in the Orleans cluster.
public record SiloInfo(string SiloName, string SiloAddress);