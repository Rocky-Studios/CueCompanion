using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

namespace CueCompanion.Client;

internal class Program
{
    private static async Task Main(string[] args)
    {
        WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<ShowService>();
        builder.Services.AddScoped<LocalStorageService>();

        builder.Services.AddMudServices();

        await builder.Build().RunAsync();
    }
}