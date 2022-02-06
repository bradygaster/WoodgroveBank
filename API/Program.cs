using Orleans;
using WoodgroveBank.Abstractions;
using WoodgroveBank.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddWoodvilleBankSilo();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    await grainFactory.GetGrain<ICustomerGrain>(0).GetCustomers()
)
.WithName("GetCustomers");

/// <summary>
/// Create a new customer.
/// </summary>
app.MapPost("/customers", async (IGrainFactory grainFactory, Customer customer) =>
{
    try
    {
        var result = await grainFactory.GetGrain<ICustomerGrain>(0).SaveCustomer(customer);
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
app.MapPost("/customer/{id}/accounts", async (IGrainFactory grainFactory, int id, NewAccountRequest request) =>
{
    try
    {
        var customer = await grainFactory.GetGrain<ICustomerGrain>(id).GetCustomer();

        var result = await grainFactory.GetGrain<IBankGrain>(Guid.Empty)
            .OpenAccount(request.Name, customer, request.AccountType, request.Amount);

        return Results.Created($"/accounts/{result.Id}", result);
    }
    catch (Exception ex)
    {
        return Results.Conflict(ex.Message);
    }
})
.WithName("CreateAccount")
.Produces(StatusCodes.Status409Conflict)
.Produces<Account>(StatusCodes.Status201Created);

// run the api
app.Run();

public record NewAccountRequest(string Name, AccountType AccountType, decimal Amount);