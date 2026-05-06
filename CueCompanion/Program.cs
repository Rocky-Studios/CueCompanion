using BitzArt.Blazor.Cookies;
using CueCompanion.Hubs;
using CueCompanion.Services;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.ResponseCompression;
using MudBlazor.Services;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CueCompanion;

public class Program
{
    public static void Main(string[] args)
    {
        PDF();
        return;
        DatabaseHandler.Init();

        ShowManager.Init();
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        Console.WriteLine("Starting Cue Companion...");
        Console.WriteLine("If you haven't already, specific application URL with the ASPNETCORE_URLS environment variable");

        // Add services to the container.
        StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);
        builder.Services.AddRazorComponents()
               .AddInteractiveServerComponents();
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddSingleton<AuthHub>();

        builder.Services.AddScoped<UserManagementService>();
        builder.Services.AddSingleton<UserManagementHub>();

        builder.Services.AddScoped<ShowService>();
        builder.Services.AddSingleton<ShowHub>();

        builder.Services.AddScoped<ChatService>();
        builder.Services.AddSingleton<ChatHub>();

        builder.Services.AddScoped<ConfigService>();
        builder.Services.AddSingleton<ConfigHub>();

        builder.Services.AddScoped<NotesService>();
        builder.Services.AddSingleton<NotesHub>();

        builder.Services.AddScoped<SimpleDialogService>();

        builder.Services.AddSignalR();
        builder.Services.AddBlazorCookiesServerSideServices();
        builder.Services.AddMudServices();

        builder.Services.AddResponseCompression(opts =>
                                                {
                                                    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                                                        ["application/octet-stream"]);
                                                });

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");

            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();
        app.UseAntiforgery();
        app.MapHub<AuthHub>("/api/auth");
        app.MapHub<UserManagementHub>("/api/user-management");
        app.MapHub<ShowHub>("/api/show");
        app.MapHub<ChatHub>("/api/chat");
        app.MapHub<ConfigHub>("/api/config");
        app.MapHub<NotesHub>("/api/notes");

        app.UseStaticFiles();
        app.MapStaticAssets();
        app.MapRazorComponents<App>()
           .AddInteractiveServerRenderMode();

        app.Run();
    }

    //private static string? GetArgValue(string[] args, string key)
    //{
    //    for (int i = 0; i < args.Length; i++)
    //    {
    //        // Supports: -url http://... and -url=http://...
    //        if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
    //        {
    //            if (i + 1 < args.Length) return args[i + 1];
    //            return null;
    //        }
    //
    //        if (args[i].StartsWith(key + "=", StringComparison.OrdinalIgnoreCase))
    //            return args[i].Substring(key.Length + 1);
    //    }
    //
    //    return null;
    //}

    private static void PDF()
    {
        Settings.License = LicenseType.Community;
        Document.Create(container =>
                        {
                            container.Page(page =>
                                           {
                                               page.Size(PageSizes.A4);
                                               page.Margin(2, Unit.Centimetre);
                                               page.PageColor(Colors.White);
                                               page.DefaultTextStyle(x => x.FontSize(20));

                                               page.Header()
                                                   .Text("CTHS Media Centre")
                                                   .AlignCenter()
                                                   .SemiBold().FontSize(16).FontColor(Colors.Black);

                                               page.Content()
                                                   .PaddingVertical(1, Unit.Centimetre)
                                                   .Column(x =>
                                                           {
                                                               x.Spacing(20);

                                                               x.Item()
                                                                .Text("CTHS Media Centre")
                                                                .AlignCenter()
                                                                .Bold().FontSize(36).FontColor(Colors.Red.Medium);

                                                               x.Item()
                                                                .Text("Confidential information enclosed")
                                                                .AlignCenter()
                                                                .SemiBold().FontSize(20).FontColor(Colors.Black);

                                                               x.Item()
                                                                .Text("MADD Night 2026")
                                                                .AlignCenter()
                                                                .SemiBold().FontSize(28).FontColor(Colors.Black);
                                                           });

                                               page.Footer()
                                                   .AlignCenter()
                                                   .Text(x =>
                                                         {
                                                             x.Span("Page ");
                                                             x.CurrentPageNumber();
                                                         });
                                           });
                        })
                .GeneratePdf("hello.pdf");
    }
}