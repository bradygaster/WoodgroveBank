var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddWoodgroveBankSilo(silo => silo.AddMemoryStreams("BANK"));
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<CustomerReceivedStreamHandler>();
builder.Services.AddSingleton<TransactionProcessedStreamHandler>();
builder.Services.AddSingleton<StreamProcessingWorker>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<StreamProcessingWorker>());

var app = builder.Build();

app.MapDefaultEndpoints();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
