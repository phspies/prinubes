using Prinubes.Common.Helpers;
using Prinubes.Common.Models;
using Prinubes.Common.Models.Enums;
using Prinubes.PlatformWorker.Datamodels;

namespace Prinubes.PlatformWorker.BackgroundWorkers
{
    public class GlobalNetworkPlatformBackgroundWorker : BackgroundService, IDisposable
    {
        private SynchronizedCollection<Tuple<Guid, string, CancellationTokenSource, Task>> platformProcesses = new SynchronizedCollection<Tuple<Guid, string, CancellationTokenSource, Task>>();
        private readonly ILogger<GlobalNetworkPlatformBackgroundWorker> logger;
        private readonly PrinubesPlatformWorkerDBContext dbContext;
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public GlobalNetworkPlatformBackgroundWorker(IServiceProvider _serviceProvider)
        {
            logger = ServiceActivator.GetRequiredService<ILogger<GlobalNetworkPlatformBackgroundWorker>>(_serviceProvider);
            dbContext = ServiceActivator.GetRequiredService<PrinubesPlatformWorkerDBContext>(_serviceProvider);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(async () =>
        {
            logger.LogInformation("GlobalNetworkPlatformThreadPool Thread running.");

            //load all known compute platforms in database
            foreach (var platform in dbContext.NetworkPlatforms.Where(x => x.Enabled == true))
            {
                await AddPlatformAsync(platform.Id);
            }
        });
        public async Task AddPlatformAsync(Guid NetworkPlatformID)
        {
            await semaphoreSlim.WaitAsync();
            var platform = dbContext.NetworkPlatforms.Single(x => x.Id.Equals(NetworkPlatformID));
            CancellationTokenSource cts = new CancellationTokenSource();
            platformProcesses.Add(Tuple.Create(platform.Id, platform.Platform, cts, Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(2000);
                        logger.LogInformation($"GlobalNetworkPlatformThreadPool Thread is running: {platform.Id}");

                    }
                    catch (Exception ex)
                    {

                    }
                }
                return;
            }, cts.Token)));
            semaphoreSlim.Release();
        }
        public async Task StopPlatformAsync(Guid NetworkPlatformID)
        {
            logger.LogInformation($"GlobalNetworkPlatformThreadPool Thread is stopping: {NetworkPlatformID}");
            await semaphoreSlim.WaitAsync();

            if (platformProcesses.Any(x => x.Item1.Equals(NetworkPlatformID)))
            {
                var process = platformProcesses.Single(x => x.Item1.Equals(NetworkPlatformID));
                process.Item3.Cancel();
                while (!process.Item4.IsCompleted)
                {
                    logger.LogInformation($"GlobalNetworkPlatformThreadPool waiting to stop: {NetworkPlatformID} - Is Completed: {process.Item4.IsCompleted}");
                    await Task.Delay(1000);
                }
                platformProcesses.Remove(process);
            }
            logger.LogInformation($"GlobalNetworkPlatformThreadPool Thread stopped: {NetworkPlatformID}");
            semaphoreSlim.Release();

        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("GlobalNetworkPlatformThreadPool is stopping.");
            for (int i = 0; i < platformProcesses.Count; i++)
            {
                await StopPlatformAsync(platformProcesses[i].Item1);
            }
            logger.LogInformation("GlobalNetworkPlatformThreadPool stopped.");
        }

        public override void Dispose()
        {
            // Do resource cleanup
        }
    }
}
