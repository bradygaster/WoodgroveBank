var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AsOrleansClient();
builder.Services.AddGrpc();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGrpcService<Scaler.Services.ExternalScalerService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
