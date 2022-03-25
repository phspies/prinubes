using Microsoft.Extensions.DependencyInjection;

namespace Prinubes.Common.Helpers
{
    public class ServiceProviderFactory
    {
        public static ServiceProvider Build()
        {

            return new ServiceCollection().BuildServiceProvider();

        }
    }
}
