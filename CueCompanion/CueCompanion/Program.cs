using CueCompanion.Client;
using CueCompanion.Components;
using CueCompanion.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using _Imports = CueCompanion.Client._Imports;

namespace CueCompanion;

public class Program
{
    public static UserManager UserManager = new();

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
        builder.Services.AddSingleton<CounterService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<CueHub>();
        builder.Services.AddSingleton<AuthHub>();

        WebApplication app = builder.Build();

        app.UseResponseCompression();
        app.MapHub<CueHub>("/cueHub");
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

        app.MapPost("/api/cue/next", async ([FromServices] CueHub hub) =>
        {
            await hub.UpdateCueNumber(hub.GetState().CurrentCueNumber + 1);
            return Results.Ok();
        });

        app.MapPost("/api/cue/prev", async ([FromServices] CueHub hub) =>
        {
            await hub.UpdateCueNumber(hub.GetState().CurrentCueNumber - 1);
            return Results.Ok();
        });

        app.MapPost("/api/cue/set/{number:int}", async (int number, [FromServices] CueHub hub) =>
        {
            await hub.UpdateCueNumber(number);
            return Results.Ok();
        });


        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(_Imports).Assembly);


        app.Run();
    }
}