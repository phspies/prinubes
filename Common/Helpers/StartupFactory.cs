using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Prinubes.Common.Helpers
{
    public class StartupFactory
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
     
    }
}
