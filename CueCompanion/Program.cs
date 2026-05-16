using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using BitzArt.Blazor.Cookies;
using CueCompanion.Hubs;
using CueCompanion.Services;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.ResponseCompression;
using MudBlazor.Services;
using QuestPDF.Infrastructure;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Settings = QuestPDF.Settings;

namespace CueCompanion;

public class Program
{
    private static readonly IDeserializer _yamlDeserializer = new DeserializerBuilder()
                                                             .WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

    private static readonly ISerializer _yamlSerializer = new SerializerBuilder()
                                                         .WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

    public static void Main(string[] args)
    {
        Settings.License = LicenseType.Community;
        DatabaseHandler.Init();

        ShowManager.Init();
        ProgramConfig cfg           = new();
        bool          cfgFileExists = File.Exists("config.yaml");
        if (cfgFileExists)
        {
            string yaml = File.ReadAllText("config.yaml");
            cfg = DeserializeConfig(yaml);
        }

        File.WriteAllText("config.yaml", _yamlSerializer.Serialize(cfg));


        Console.WriteLine("Starting Cue Companion...");

        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        if (cfg.Urls.Length > 0) builder.WebHost.UseUrls(cfg.Urls);

        builder.WebHost.ConfigureKestrel(options =>
                                         {
                                             if (!string.IsNullOrWhiteSpace(cfg.CertificatePath))
                                             {
                                                 X509Certificate2 certificate = X509CertificateLoader.LoadPkcs12FromFile(
                                                     cfg.CertificatePath,
                                                     cfg.CertificatePassword);

                                                 options.ConfigureHttpsDefaults(httpsOptions => { httpsOptions.ServerCertificate = certificate; });
                                             }
                                         });

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

        app.UseStaticFiles();
        app.MapStaticAssets();
        app.MapRazorComponents<App>()
           .AddInteractiveServerRenderMode();

        app.Run();
    }

    private static ProgramConfig DeserializeConfig(string yaml)
    {
        string normalizedYaml = Regex.Replace(yaml, @"\buRLs\b", "urls", RegexOptions.CultureInvariant);
        return _yamlDeserializer.Deserialize<ProgramConfig>(normalizedYaml) ?? new ProgramConfig();
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
}