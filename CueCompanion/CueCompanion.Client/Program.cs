using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace CueCompanion.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddSingleton<CounterService>();

            await builder.Build().RunAsync();
        }
    }
}
