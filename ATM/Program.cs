using ATM.ApiClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient("api", (client) =>
{
    var baseURL = (Environment.GetEnvironmentVariable("BASE_URL") 
        ?? "http://localhost") + ":" + (Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3500");

    //client.BaseAddress = new Uri("http://localhost:5000");
    client.BaseAddress = new Uri(baseURL);
    client.DefaultRequestHeaders.Add("dapr-app-id", "api");
});
builder.Services.AddSingleton<WoodgroveBankAPIClient>();

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
