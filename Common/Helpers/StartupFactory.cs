using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Prinubes.Common.Models;
using StackExchange.Redis;

namespace Prinubes.Common.Helpers
{
    public static class StartupFactory
    {
        public static Action<MvcNewtonsoftJsonOptions> MvcNewtonsoftJsonOptionsBuilder()
        {
            return new Action<MvcNewtonsoftJsonOptions>(opt =>
            {
                opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                opt.SerializerSettings.Converters = new JsonConverter[] {
                    new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ" },
                    new StringEnumConverter() };
            });
        }

        public static Action<ILoggingBuilder> LoggingBuilder()
        {
            return new Action<ILoggingBuilder>(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddConsole(options => { options.Format = ConsoleLoggerFormat.Systemd; });
                builder.AddSystemdConsole(options =>
                {
                    options.IncludeScopes = false;
                    options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
                });
            });
        }
        public static IServiceCollection CachingBuilder(this IServiceCollection services, ServiceSettings serviceSettings)
        {
            services.AddStackExchangeRedisCache(builder =>
            {
                string? assemblyName = System.Reflection.Assembly.GetEntryAssembly()?.GetName()?.Name;
                ArgumentNullException.ThrowIfNull(assemblyName);
                builder.InstanceName = $"{assemblyName.ToLower()}-";
                builder.ConfigurationOptions = new ConfigurationOptions()
                {
                    EndPoints = { serviceSettings.REDIS_CACHE_HOST, serviceSettings.REDIS_CACHE_PORT.ToString() },
                    AllowAdmin = true,
                    ClientName = assemblyName
                };
            });
            return services;
        }

    }
}
