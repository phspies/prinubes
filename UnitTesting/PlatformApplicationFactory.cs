using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prinubes.Common.Models;
using Prinubes.Platforms.Datamodels;
using System;
using System.Linq;
using System.Net.Http;
using Prinubes.Platform;

namespace UnitTesting
{
    class PlatformApplicationFactory : WebApplicationFactory<PlatformProgram>
    {
        public PrinubesPlatformDBContext? DBContext;
        public HttpClient? Client;
        public IHost? WebHost;
        private ServiceSettings serviceSettings;

        public PlatformApplicationFactory(ServiceSettings _serviceSettings)
        {
            serviceSettings = _serviceSettings;
        }
        protected override IHost CreateHost(IHostBuilder builder)
        {
            WebHost = builder.Build();
            WebHost.Start();
            Client = WebHost.GetTestClient();
            return WebHost;
        }
        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            return base.CreateServer(builder);
        }
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PrinubesPlatformDBContext>));
                if (dbDescriptor != null)
                {
                    services.Remove(dbDescriptor);
                }
                //setup database connection and logging         
                services.AddDbContextPool<PrinubesPlatformDBContext>((serviceProvider, optionsBuilder) =>
                {
                    optionsBuilder.UseMySql(serviceSettings.GetMysqlConnection().ConnectionString, ServerVersion.AutoDetect(serviceSettings.GetMysqlConnection().ConnectionString));
                });

                DBContext = services.BuildServiceProvider().GetRequiredService<PrinubesPlatformDBContext>();
                if (DBContext.Exists())
                {
                    DBContext.Database.EnsureDeleted();
                }
                DBContext.Database.Migrate();
            });
            builder.UseTestServer(options => { options.BaseAddress = new Uri("http://localhost:9001"); });
        }
    }
}
