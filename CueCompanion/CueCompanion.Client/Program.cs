using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace CueCompanion.Client;

internal class Program
{
    private static async Task Main(string[] args)
    {
        WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Services.AddSingleton<CounterService>();

        await builder.Build().RunAsync();
    }
}