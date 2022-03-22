using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Text;

namespace Prinubes.Common.Helpers
{
    static public class DistributedCaching
    {
        public static DistributedCacheEntryOptions BuildOptions()
        {
            return new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(DateTime.Now.AddHours(1));
        }
        public static async Task SetCachingAsync(this IDistributedCache cachingConstruct, object objectToStore, string cachingKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            var encodedValue = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(objectToStore,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));
            await cachingConstruct.SetAsync(cachingKey, encodedValue, BuildOptions(), cancellationToken);
        }
        public static void SetCaching(this IDistributedCache cachingConstruct, object objectToStore, string cachingKey)
        {
            var encodedValue = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(objectToStore,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));
            cachingConstruct.Set(cachingKey, encodedValue, BuildOptions());
        }
    }
}
