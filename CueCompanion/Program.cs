using CueCompanion.Components;
using CueCompanion.Hubs;
using CueCompanion.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using BitzArt.Blazor.Cookies;
using MudBlazor.Services;

namespace CueCompanion
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DatabaseHandler.Init();
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            //builder.WebHost.UseUrls("http://0.0.0.0:5277");  // SIGNIFICANTLY SLOWS DOWN THE APP SO ONLY USE IF ABSOLUTELY NECESSARY
            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddSingleton<AuthHub>();
            
            builder.Services.AddScoped<UserManagementService>();
            builder.Services.AddSingleton<UserManagementHub>();
            
            builder.Services.AddSignalR();
            builder.Services.AddBlazorCookiesServerSideServices();
            builder.Services.AddMudServices();

            builder.Services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    [ "application/octet-stream" ]);
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
            app.MapHub<AuthHub>("/auth");
            app.MapHub<UserManagementHub>("/user-management");

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
