using Microsoft.AspNetCore.Mvc;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;
using WoodgroveBank.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.AddWoodvilleBankSilo(useDashboard: true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.MapGet("/cluster-details", async ([FromServices] IGrainFactory grainFactory) =>
{
    var managementGrain = grainFactory.GetGrain<IManagementGrain>(0);
    var statistics = await managementGrain.GetDetailedGrainStatistics();

    var result = statistics.Select(_ => new GrainInfo(_.GrainType, _.GrainIdentity.IdentityString, _.SiloAddress.ToGatewayUri().AbsoluteUri));

    return Results.Ok(result);
});

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();

public record GrainInfo(string Type, string PrimaryKey, string SiloName);