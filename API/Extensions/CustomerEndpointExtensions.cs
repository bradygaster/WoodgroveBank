namespace Microsoft.AspNetCore.Builder;

public static class CustomerEndpointExtensions
{
    public static WebApplication MapCustomerEndpoints(this WebApplication app)
    {
        /// <summary>
        /// Get a list of all the customers.
        /// </summary>
        app.MapGet("/customers", async (IClusterClient clusterClient) =>
            await clusterClient.GetGrain<IBankGrain>(Guid.Empty).GetCustomers()
        )
        .WithTags("Customers")
        .WithName("GetCustomers");

        /// <summary>
        /// Create a new customer.
        /// </summary>
        app.MapPost("/customers", async (IClusterClient clusterClient, Customer customer) =>
        {
            try
            {
                var result = await clusterClient.GetGrain<ICustomerGrain>(customer.Id).SaveCustomer(customer);
                await clusterClient.GetGrain<IBankGrain>(Guid.Empty).UpdateCustomerIndex(customer);
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
        app.MapGet("/customers/{id}/accounts", async (IClusterClient clusterClient, Guid id) =>
        {
            var customerGrain = clusterClient.GetGrain<ICustomerGrain>(id);
            return Results.Ok(await customerGrain.GetAccounts());
        })
        .WithTags("Accounts")
        .WithName("GetCustomerAccounts")
        .Produces<List<Account>>();

        return app;
    }
}
