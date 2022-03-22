using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
