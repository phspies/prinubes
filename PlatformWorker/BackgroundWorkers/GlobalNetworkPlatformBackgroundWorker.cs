using Prinubes.Common.Helpers;
using Prinubes.Common.Models;
using Prinubes.Common.Models.Enums;
using Prinubes.PlatformWorker.Datamodels;

namespace Prinubes.PlatformWorker.BackgroundWorkers
{
    public class GlobalNetworkPlatformBackgroundWorker : BackgroundService, IDisposable
    {
        private SynchronizedCollection<Tuple<Guid, string, CancellationTokenSource, Task>> processes = new SynchronizedCollection<Tuple<Guid, string, CancellationTokenSource, Task>>();
        private readonly ILogger<GlobalNetworkPlatformBackgroundWorker> logger;
        private readonly PrinubesPlatformWorkerDBContext dbContext;
        private ServiceSettings settingsContext;
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public GlobalNetworkPlatformBackgroundWorker(IServiceProvider _serviceProvider)
        {
            logger = ServiceActivator.GetRequiredService<ILogger<GlobalNetworkPlatformBackgroundWorker>>(_serviceProvider);
            dbContext = ServiceActivator.GetRequiredService<PrinubesPlatformWorkerDBContext>(_serviceProvider);
            settingsContext = ServiceActivator.GetRequiredService<ServiceSettings>(_serviceProvider);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(async () =>
        {
            logger.LogInformation("GlobalNetworkPlatformThreadPool Thread running.");

            //load all known compute platforms in database
            var knownPlatforms = dbContext.NetworkPlatforms.Where(x => x.Enabled == true && x.state != PlatformState.Error);
            foreach (var platform in knownPlatforms)
            {
                await AddPlatformAsync(platform.Id);
            }

            //loop until we 
            while (true)
            {
                foreach (var process in processes)
                {
                    logger.LogInformation($"GlobalNetworkPlatformThreadPool Thread is running: {process.Item2}");
                }
                Thread.Sleep((settingsContext.BACKGROUND_WORKER_INTERVAL ?? 10) * 1000);
                await Task.Yield();
            }
        });
        public async Task AddPlatformAsync(Guid NetworkPlatformID)
        {
            await semaphoreSlim.WaitAsync();
            var platform = dbContext.NetworkPlatforms.Single(x => x.Id.Equals(NetworkPlatformID));
            CancellationTokenSource cts = new CancellationTokenSource();
            processes.Add(Tuple.Create(platform.Id, platform.Platform, cts, Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
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
        public async Task StopPlatformAsync(Guid NetworkPlatformID)
        {
            logger.LogInformation($"GlobalNetworkPlatformThreadPool Thread is stopping: {NetworkPlatformID}");
            await semaphoreSlim.WaitAsync();

            if (processes.Any(x => x.Item1.Equals(NetworkPlatformID)))
            {
                var process = processes.Single(x => x.Item1.Equals(NetworkPlatformID));
                process.Item3.Cancel();
                while (!process.Item4.IsCompleted)
                {
                    logger.LogInformation($"GlobalNetworkPlatformThreadPool waiting to stop: {NetworkPlatformID} - Is Completed: {process.Item4.IsCompleted}");
                    Thread.Sleep(1000);
                }
                processes.Remove(processes.Single(x => x.Item1.Equals(NetworkPlatformID)));
            }
            logger.LogInformation($"GlobalNetworkPlatformThreadPool Thread stopped: {NetworkPlatformID}");
            semaphoreSlim.Release();

        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("GlobalNetworkPlatformThreadPool is stopping.");
            for (int i = 0; i < processes.Count; i++)
            {
                await StopPlatformAsync(processes[i].Item1);
            }
            logger.LogInformation("GlobalNetworkPlatformThreadPool stopped.");
        }

        public void Dispose()
        {
            // Do resource cleanup
        }
    }
}
