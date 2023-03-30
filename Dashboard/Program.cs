var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.AddWoodgroveBankSilo(siloBuilder => {
    siloBuilder.UseDashboard();
});
builder.Services.AddServicesForSelfHostedDashboard();

var app = builder.Build();
app.UseOrleansDashboard();
app.Run();

