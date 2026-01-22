using CueCompanion.Client;
using CueCompanion.Components;
using CueCompanion.Hubs;
using Microsoft.AspNetCore.ResponseCompression;
using MudBlazor.Services;
using _Imports = CueCompanion.Client._Imports;

namespace CueCompanion;

public class Program
{
    public static ConnectionManager ConnectionManager = new();

    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveWebAssemblyComponents()
            .AddInteractiveServerComponents();


        builder.Services.AddSignalR();

        builder.Services.AddResponseCompression(opts =>
        {
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                ["application/octet-stream"]);
        });
        // CueCompanion/CueCompanion/Program.cs
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<ShowService>();
        builder.Services.AddSingleton<ShowHub>();
        builder.Services.AddScoped<AuthHub>();
        builder.Services.AddScoped<LocalStorageService>();

        builder.Services.AddMudServices();

        WebApplication app = builder.Build();

        app.UseResponseCompression();
        app.MapHub<ShowHub>("/cueHub");
        app.MapHub<AuthHub>("/authHub");


        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();

        app.UseAntiforgery();

        // API endpoints

        //app.MapPost("/api/cue/next", async ([FromServices] CueHub hub) =>
        //{
        //    await hub.UpdateCueNumber(hub.GetState().CurrentCueNumber + 1);
        //    return Results.Ok();
        //});
        //
        //app.MapPost("/api/cue/prev", async ([FromServices] CueHub hub) =>
        //{
        //    await hub.UpdateCueNumber(hub.GetState().CurrentCueNumber - 1);
        //    return Results.Ok();
        //});
        //
        //app.MapPost("/api/cue/set/{number:int}", async (int number, [FromServices] CueHub hub) =>
        //{
        //    await hub.UpdateCueNumber(number);
        //    return Results.Ok();
        //});


        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(_Imports).Assembly);

        new Thread(() =>
        {
            while (true)
            {
                Console.Write("Enter command: ");
                string? command = Console.ReadLine();
                if (command == "exit")
                {
                    app.Lifetime.StopApplication();
                    break;
                }

                if (command is "listConnections")
                {
                    Console.WriteLine("Connections:");
                    foreach (Connection connection in ConnectionManager.GetConnections())
                        Console.WriteLine(
                            $"\t\"{connection.ConnectionName}\" (Passkey: \"{connection.ConnectionPasskey}\")");
                }
            }
        }).Start();

        app.Run();
    }
}