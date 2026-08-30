using DungeonRush.Client.Blazor.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5001") }); // замени на URL бэкенда
builder.Services.AddScoped<AuthState>();
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<GameLoop>();

await builder.Build().RunAsync();