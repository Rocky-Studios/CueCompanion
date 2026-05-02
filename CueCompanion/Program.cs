using BitzArt.Blazor.Cookies;
using CueCompanion.Hubs;
using CueCompanion.Services;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.ResponseCompression;
using MudBlazor.Services;

namespace CueCompanion
{
    public class Program
    {
        public const string localhostURL = "http://127.0.0.1:7081";

        public static void Main(string[] args)
        {
            DatabaseHandler.Init();

            ShowManager.Init();
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            string?               main    = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
            builder.WebHost.UseUrls(main ?? "https://localhost:7082", localhostURL);
            Console.WriteLine("Starting Cue Companion...");
            Console.WriteLine("Listening on: " + main);

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

            builder.Services.AddHttpClient("DiagClient")
                   .ConfigurePrimaryHttpMessageHandler(() =>
                                                       {
                                                           return new HttpClientHandler
                                                           {
                                                               ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) =>
                                                               {
                                                                   Console.WriteLine("CERT ERROR calling: " + msg.RequestUri);
                                                                   return false; // do NOT allow it, we want to see the URL
                                                               },
                                                           };
                                                       });


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
                //app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

            //app.UseHttpsRedirection();
            app.UseAntiforgery();
            app.MapHub<AuthHub>("/api/auth");
            app.MapHub<UserManagementHub>("/api/user-management");
            app.MapHub<ShowHub>("/api/show");
            app.MapHub<ChatHub>("/api/chat");
            app.MapHub<ConfigHub>("/api/config");
            app.MapHub<NotesHub>("/api/notes");

            app.UseStaticFiles();
            app.MapStaticAssets();
            app.Use((context, next) =>
                    {
                        context.Request.Scheme = "http";
                        return next();
                    });

            app.MapRazorComponents<App>()
               .AddInteractiveServerRenderMode();

            app.Run();
        }

        private static string? GetArgValue(string[] args, string key)
        {
            for (int i = 0; i < args.Length; i++)
            {
                // Supports: -url http://... and -url=http://...
                if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < args.Length) return args[i + 1];
                    return null;
                }

                if (args[i].StartsWith(key + "=", StringComparison.OrdinalIgnoreCase))
                    return args[i].Substring(key.Length + 1);
            }

            return null;
        }
    }
}