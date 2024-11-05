using Orleans.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AsOrleansSilo(silo =>
{
    silo.Configure<SiloOptions>(options =>
        options.SiloName = $"transactionsilo_{Path.GetFileNameWithoutExtension(Path.GetRandomFileName())}");
});
var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => "Hello World!");

app.Run();
