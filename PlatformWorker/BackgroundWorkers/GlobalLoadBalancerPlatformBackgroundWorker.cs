using Prinubes.Common.Helpers;
using Prinubes.Common.Models;
using Prinubes.Common.Models.Enums;
using Prinubes.PlatformWorker.Datamodels;

namespace Prinubes.PlatformWorker.BackgroundWorkers
{
    public class GlobalLoadBalancerPlatformBackgroundWorker : BackgroundService, IDisposable
    {
        private SynchronizedCollection<Tuple<Guid, string, CancellationTokenSource, Task>> processes = new SynchronizedCollection<Tuple<Guid, string, CancellationTokenSource, Task>>();
        private readonly ILogger<GlobalLoadBalancerPlatformBackgroundWorker> logger;
        private readonly PrinubesPlatformWorkerDBContext dbContext;
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public GlobalLoadBalancerPlatformBackgroundWorker(IServiceProvider _serviceProvider)
        {
            logger = ServiceActivator.GetRequiredService<ILogger<GlobalLoadBalancerPlatformBackgroundWorker>>(_serviceProvider);
            dbContext = ServiceActivator.GetRequiredService<PrinubesPlatformWorkerDBContext>(_serviceProvider);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(async () =>
        {
            logger.LogInformation("GlobalLoadBalancerPlatformThreadPool Thread running.");

            //load all known compute platforms in database
            foreach (var platform in dbContext.LoadBalancerPlatforms.Where(x => x.Enabled == true))
            {
                await AddPlatformAsync(platform.Id);
            }
        });
        public async Task AddPlatformAsync(Guid LoadBalancerPlatformID)
        {
            var platform = dbContext.LoadBalancerPlatforms.Single(x => x.Id.Equals(LoadBalancerPlatformID));
            CancellationTokenSource cts = new CancellationTokenSource();
            await semaphoreSlim.WaitAsync();
            processes.Add(Tuple.Create(platform.Id, platform.Platform, cts, Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        logger.LogInformation($"GlobalLoadBalancerPlatformThreadPool Thread is running: {platform.Id}");
                        await Task.Delay(2000);
                    }
                    catch (Exception ex)
                    {

                    }
                }
                return;
            }, cts.Token)));
            semaphoreSlim.Release();
        }
        public async Task StopPlatformAsync(Guid LoadBalancerPlatformID)
        {
            logger.LogInformation($"GlobalLoadBalancerPlatformThreadPool Thread is stopping: {LoadBalancerPlatformID}");
            await semaphoreSlim.WaitAsync();

            if (processes.Any(x => x.Item1.Equals(LoadBalancerPlatformID)))
            {
                var process = processes.Single(x => x.Item1.Equals(LoadBalancerPlatformID));
                process.Item3.Cancel();
                while (!process.Item4.IsCompleted)
                {
                    logger.LogInformation($"GlobalLoadBalancerPlatformThreadPool waiting to stop: {LoadBalancerPlatformID} - Is Completed: {process.Item4.IsCompleted}");
                    await Task.Delay(1000);
                }
                processes.Remove(process);
            }
            logger.LogInformation($"GlobalLoadBalancerPlatformThreadPool Thread stopped: {LoadBalancerPlatformID}");
            semaphoreSlim.Release();
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("GlobalLoadBalancerPlatformThreadPool is stopping.");
            for (int i = 0; i < processes.Count; i++)
            {
                await StopPlatformAsync(processes[i].Item1);
            }
            logger.LogInformation("GlobalLoadBalancerPlatformThreadPool stopped.");
        }

        public override void Dispose()
        {
            // Do resource cleanup
        }
    }
}
