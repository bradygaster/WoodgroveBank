var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AsOrleansSilo();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<CustomerReceivedStreamHandler>();
builder.Services.AddSingleton<TransactionProcessedStreamHandler>();

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
