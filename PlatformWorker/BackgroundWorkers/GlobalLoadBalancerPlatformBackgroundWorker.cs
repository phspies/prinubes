using Prinubes.Common.Models;
using Prinubes.Common.Models.Enums;
using Prinubes.PlatformWorker.Datamodels;

namespace Prinubes.PlatformWorker.BackgroundWorkers
{
    public class GlobalLoadBalancerPlatformBackgroundWorker : BackgroundService, IDisposable
    {
        private SynchronizedCollection<Tuple<Guid, string, CancellationTokenSource, Task>> processes = new SynchronizedCollection<Tuple<Guid, string, CancellationTokenSource, Task>>();
        private readonly ILogger<GlobalLoadBalancerPlatformBackgroundWorker> logger;
        private readonly PrinubesPlatformWorkerDBContext DBContext;
        private ServiceSettings settings;
        object guard = new object();


        public GlobalLoadBalancerPlatformBackgroundWorker(ILogger<GlobalLoadBalancerPlatformBackgroundWorker> _logger, IServiceProvider _serviceProvider)
        {
            var scope = _serviceProvider.CreateScope();
            logger = _logger;
            DBContext = scope.ServiceProvider.GetRequiredService<PrinubesPlatformWorkerDBContext>();
            settings = scope.ServiceProvider.GetRequiredService<ServiceSettings>();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(async () =>
        {
            logger.LogInformation("GlobalLoadBalancerPlatformThreadPool Thread running.");

            //load all known compute platforms in database
            var knownPlatforms = DBContext.LoadBalancerPlatforms.Where(x => x.Enabled == true && x.state != PlatformState.Error);
            foreach (var platform in knownPlatforms)
            {
                AddPlatform(platform.Id);
            }

            //loop until we 
            while (true)
            {
                foreach (var process in processes)
                {
                    logger.LogInformation($"GlobalLoadBalancerPlatformThreadPool Thread is running: {process.Item2}");
                }
                Thread.Sleep((settings.BACKGROUND_WORKER_INTERVAL ?? 10) * 1000);
                await Task.Yield();
            }
        });
        public void AddPlatform(Guid LoadBalancerPlatformID)
        {
            var platform = DBContext.LoadBalancerPlatforms.Single(x => x.Id.Equals(LoadBalancerPlatformID));
            CancellationTokenSource cts = new CancellationTokenSource();
            lock (guard)
            {
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
            }
        }
        public void StopPlatform(Guid LoadBalancerPlatformID)
        {
            logger.LogInformation($"GlobalLoadBalancerPlatformThreadPool Thread is stopping: {LoadBalancerPlatformID}");
            lock (guard)
            {
                if (processes.Any(x => x.Item1.Equals(LoadBalancerPlatformID)))
                {
                    var process = processes.Single(x => x.Item1.Equals(LoadBalancerPlatformID));
                    process.Item3.Cancel();
                    while (!process.Item4.IsCompleted)
                    {
                        logger.LogInformation($"GlobalLoadBalancerPlatformThreadPool waiting to stop: {LoadBalancerPlatformID} - Is Completed: {process.Item4.IsCompleted}");
                        Thread.Sleep(1000);
                    }
                    processes.Remove(processes.Single(x => x.Item1.Equals(LoadBalancerPlatformID)));
                }
                logger.LogInformation($"GlobalLoadBalancerPlatformThreadPool Thread stopped: {LoadBalancerPlatformID}");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("GlobalLoadBalancerPlatformThreadPool is stopping.");
            for (int i = 0; i < processes.Count; i++)
            {
                StopPlatform(processes[i].Item1);
            }
            logger.LogInformation("GlobalLoadBalancerPlatformThreadPool is stopped.");
        }

        public void Dispose()
        {
            // Do resource cleanup
        }
    }
}
