using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Helpers;
using Prinubes.Common.Models;
using Prinubes.Common.Models.Enums;
using Prinubes.PlatformWorker.CloudLibraries.vSphere;
using Prinubes.PlatformWorker.Datamodels;
using vspheresdk.Appliance.Models;

namespace Prinubes.PlatformWorker.BackgroundWorkers
{
    public class GlobalComputePlatformBackgroundWorker : BackgroundService, IDisposable
    {
        private SynchronizedCollection<Tuple<Guid, string, CancellationTokenSource, Task>> processes = new SynchronizedCollection<Tuple<Guid, string, CancellationTokenSource, Task>>();
        private readonly ILogger<GlobalComputePlatformBackgroundWorker> logger;
        private readonly PrinubesPlatformWorkerDBContext dbContext;
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public GlobalComputePlatformBackgroundWorker(IServiceProvider _serviceProvider)
        {
            logger = ServiceActivator.GetRequiredService<ILogger<GlobalComputePlatformBackgroundWorker>>(_serviceProvider);
            dbContext = ServiceActivator.GetRequiredService<PrinubesPlatformWorkerDBContext>(_serviceProvider);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(async () =>
        {
            logger.LogInformation("GlobalComputePlatformThreadPool Thread running.");

            //load all known compute platforms in database
            foreach (var platform in dbContext.ComputePlatforms.Where(x => x.Enabled == true))
            {
                await AddPlatformAsync(platform.Id);
            }
        });
        public async Task AddPlatformAsync(Guid ComputePlatformID)
        {
            await semaphoreSlim.WaitAsync();
            ComputePlatformDatabaseModel platform = dbContext.ComputePlatforms.Single(x => x.Id.Equals(ComputePlatformID));
            CancellationTokenSource cts = new CancellationTokenSource();
            processes.Add(Tuple.Create(platform.Id, platform.Platform, cts, Task.Run(async () =>
            {
                //discover new platform
                vSphereFactory vsphereFactory = new vSphereFactory(platform, dbContext, logger);
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        logger.LogInformation($"GlobalComputePlatformThreadPool Thread is running: {platform.Id}");
                        try
                        {
                            ApplianceSystemVersionVersionStructType information = await vsphereFactory.Discover();
                            logger.LogInformation($"Found platform: {information.Version}");
                        }
                        catch (Exception ex)
                        {
                            logger.LogInformation($"Cannot connect to platform: {ex.Message}");
                        }
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
        public async Task StopPlatformAsync(Guid ComputePlatformID)
        {
            logger.LogInformation($"GlobalComputePlatformThreadPool Thread is stopping: {ComputePlatformID}");
            await semaphoreSlim.WaitAsync();
            if (processes.Any(x => x.Item1.Equals(ComputePlatformID)))
            {
                var process = processes.Single(x => x.Item1.Equals(ComputePlatformID));
                process.Item3.Cancel();
                while (!process.Item4.IsCompleted)
                {
                    logger.LogInformation($"GlobalComputePlatformThreadPool waiting to stop: {ComputePlatformID} - Is Completed: {process.Item4.IsCompleted}");
                    Thread.Sleep(1000);
                }
                processes.Remove(processes.Single(x => x.Item1.Equals(ComputePlatformID)));
            }
            logger.LogInformation($"GlobalComputePlatformThreadPool Thread stopped: {ComputePlatformID}");
            semaphoreSlim.Release();
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("GlobalComputePlatformThreadPool is stopping.");
            for (int i = 0; i < processes.Count; i++)
            {
                await StopPlatformAsync(processes[i].Item1);
            }
            logger.LogInformation("GlobalComputePlatformThreadPool stopped.");
        }

        public override void Dispose()
        {
            // Do resource cleanup
        }
    }
}
