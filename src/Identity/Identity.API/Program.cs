using Identity.API.Database;
using Identity.API.Extensions;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.IO;
using System.Reflection;

namespace Identity.API
{
    public class Program
    {
        public static int Main(string[] args)
        {

            //var cn = new SqlConnection("Server=localhost;Initial Catalog=Identity;User ID=sa;Password=Phu@1234;TrustServerCertificate=true;");
            //cn.Open();


            string appName = typeof(Startup).Namespace;

            var configuration = GetConfiguration();

            Log.Logger = CreateSerilogLogger(configuration);

            try
            {
                Log.Information("Configuring web host ({ApplicationContext})...", appName);
                var host = BuildWebHost(configuration, args);

                Log.Information("Applying migrations ({ApplicationContext})...", appName);
                host.MigrateDbContext<PersistedGrantDbContext>((_, __) => { })
                    .MigrateDbContext<ApplicationDbContext>((context, services) =>
                    {
                        var env = services.GetService<IWebHostEnvironment>();
                        var logger = services.GetService<ILogger<ApplicationDbContextSeed>>();
                        var settings = services.GetService<IOptions<AppSettings>>();

                        new ApplicationDbContextSeed()
                            .SeedAsync(context, env, logger, settings)
                            .Wait();
                    })
                    .MigrateDbContext<ConfigurationDbContext>((context, services) =>
                    {
                        new ConfigurationDbContextSeed()
                            .SeedAsync(context, configuration)
                            .Wait();
                    });

                Log.Information("Starting web host ({ApplicationContext})...", appName);
                host.Run();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", appName);
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }

            IHost BuildWebHost(IConfiguration configuration, string[] args) =>
                Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.CaptureStartupErrors(false);
                    webBuilder.ConfigureAppConfiguration(x => x.AddConfiguration(configuration));
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
                    //webBuilder.UseSerilog();
                })
                .Build();

            Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
            {
                return new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .Enrich.WithProperty("ApplicationContext", appName)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day, shared: true)
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();
            }

            IConfiguration GetConfiguration()
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .AddUserSecrets(Assembly.GetAssembly(typeof(Startup)));

                var config = builder.Build();
                return builder.Build();
            }
        }
    }
}