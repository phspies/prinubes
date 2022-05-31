﻿using Prinubes.Common.Models;
using Prinubes.Common.Models.Enums;
using Prinubes.PlatformWorker.Datamodels;

namespace Prinubes.PlatformWorker.BackgroundWorkers
{
    public class GlobalComputePlatformBackgroundWorker : BackgroundService, IDisposable
    {
        private SynchronizedCollection<Tuple<Guid, string, CancellationTokenSource, Task>> processes = new SynchronizedCollection<Tuple<Guid, string, CancellationTokenSource, Task>>();
        private readonly ILogger<GlobalComputePlatformBackgroundWorker> logger;
        private readonly PrinubesPlatformWorkerDBContext DBContext;
        private ServiceSettings settings;
        object guard = new object();


        public GlobalComputePlatformBackgroundWorker(ILogger<GlobalComputePlatformBackgroundWorker> _logger, IServiceProvider _serviceProvider)
        {
            var scope = _serviceProvider.CreateScope();
            logger = _logger;
            DBContext = scope.ServiceProvider.GetRequiredService<PrinubesPlatformWorkerDBContext>();
            settings = scope.ServiceProvider.GetRequiredService<ServiceSettings>();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(async () =>
        {
            logger.LogInformation("GlobalComputePlatformThreadPool Thread running.");

            //load all known compute platforms in database
            var knownPlatforms = DBContext.ComputePlatforms.Where(x => x.Enabled == true && x.state != PlatformState.Error);
            foreach (var platform in knownPlatforms)
            {
                AddPlatform(platform.Id);
            }

            //loop until we 
            while (true)
            {
                foreach (var process in processes)
                {
                    logger.LogInformation($"GlobalComputePlatformThreadPool Thread is running: {process.Item2}");
                }
                Thread.Sleep((settings.BACKGROUND_WORKER_INTERVAL ?? 10) * 1000);
                await Task.Yield();
            }
        });
        public void AddPlatform(Guid ComputePlatformID)
        {
            lock (guard)
            {
                var platform = DBContext.ComputePlatforms.Single(x => x.Id.Equals(ComputePlatformID));
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
            }
        }
        public void StopPlatform(Guid ComputePlatformID)
        {
            logger.LogInformation($"GlobalComputePlatformThreadPool Thread is stopping: {ComputePlatformID}");
            lock (guard)
            {
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
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("GlobalComputePlatformThreadPool is stopping.");
            for (int i = 0; i < processes.Count; i++)
            {
                StopPlatform(processes[i].Item1);
            }
            logger.LogInformation("GlobalComputePlatformThreadPool is stopped.");
        }

        public void Dispose()
        {
            // Do resource cleanup
        }
    }
}
