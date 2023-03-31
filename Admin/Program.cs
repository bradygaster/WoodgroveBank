using Admin;
using WoodgroveBank.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.AddWoodgroveBankSilo(siloBuilder =>
{
    siloBuilder.ConfigureServices(services =>
    {
        services.AddSingleton<IAdminDashboardObserver, AdminDashboardObserver>();
        services.AddHostedService<AdminDashboardObserverHost>();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
