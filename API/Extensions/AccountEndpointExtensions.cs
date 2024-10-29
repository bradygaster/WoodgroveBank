namespace Microsoft.AspNetCore.Builder;

public static class AccountEndpointExtensions
{
    public static WebApplication MapAccountEndpoints(this WebApplication app)
    {
        /// <summary>
        /// Create a new account for a customer.
        /// </summary>
        app.MapPost("/accounts", async (IClusterClient clusterClient, Account account) =>
        {
            try
            {
                var customerGrain = clusterClient.GetGrain<ICustomerGrain>(account.CustomerId);
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
            async (IClusterClient clusterClient, Guid accountId, decimal amount) =>
            {
                var result = await clusterClient.GetGrain<IAccountGrain>(accountId).Deposit(amount);
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
            async (IClusterClient clusterClient, Guid accountId, decimal amount) =>
            {
                var result = await clusterClient.GetGrain<IAccountGrain>(accountId).Withdraw(amount);
                if (result) return Results.Ok();
                return Results.Unauthorized();
            })
        .WithTags("Accounts")
        .WithName("SubmitWithdrawal")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        /// <summary>
        /// Gets all of a customer's transactions for an account.
        /// </summary>
        app.MapGet("/accounts/{accountId}/transactions", async (IClusterClient clusterClient, Guid accountId) =>
        {
            var result = await clusterClient.GetGrain<IAccountGrain>(accountId).GetTransactions();
            return Results.Ok(result);
        })
        .WithTags("Accounts")
        .WithName("GetTransactions")
        .Produces<List<Transaction>>();

        return app;
    }
}
