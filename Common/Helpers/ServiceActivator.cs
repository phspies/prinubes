using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prinubes.Common.Helpers
{
    public static class ServiceActivator
    {
        internal static IServiceCollection? serviceCollection;

        /// <summary>
        /// Configure ServiceActivator with full serviceProvider
        /// </summary>
        /// <param name="serviceProvider"></param>
        public static void Configure(IServiceCollection _serviceCollection)
        {
            serviceCollection = _serviceCollection;
        }
        public static T GetRequiredService<T>(IServiceProvider? _serviceProvider = null) where T : class
        {
            IServiceProvider? sp = _serviceProvider?.CreateScope()?.ServiceProvider ?? serviceCollection?.BuildServiceProvider()?.CreateScope()?.ServiceProvider;
            ArgumentNullException.ThrowIfNull(sp);
            return sp.GetRequiredService<T>();
        }
    }
}
