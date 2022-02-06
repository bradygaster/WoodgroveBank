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

app.MapGet("/customers", async (IGrainFactory grainFactory) =>
    await grainFactory.GetGrain<IBankGrain>(Guid.Empty).GetCustomers()
)
.WithName("GetCustomers");

app.MapPost("/customers", async (IGrainFactory grainFactory, Customer customer) =>
{
    try
    {
        var result = await grainFactory.GetGrain<IBankGrain>(Guid.Empty).SaveCustomer(customer);
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

app.Run();