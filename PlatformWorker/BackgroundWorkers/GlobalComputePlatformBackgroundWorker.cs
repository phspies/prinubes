using Amib.Threading;
using Prinubes.Common.Models.Enums;
using Prinubes.PlatformWorker.Datamodels;
using System.Threading;

namespace Prinubes.PlatformWorker.BackgroundWorkers
{
    public class GlobalComputePlatformBackgroundWorker : IHostedService, IDisposable
    { 
        private List<Tuple<Guid, string, CancellationTokenSource, Task>> processes  = new List<Tuple<Guid, string, CancellationTokenSource, Task>>();
        private readonly ILogger<GlobalComputePlatformBackgroundWorker> logger;
        private readonly PrinubesPlatformWorkerDBContext DBContext;


        public GlobalComputePlatformBackgroundWorker(ILogger<GlobalComputePlatformBackgroundWorker> _logger, IServiceProvider _serviceProvider)
        {
            var scope = _serviceProvider.CreateScope();
            logger = _logger;
            DBContext = scope.ServiceProvider.GetRequiredService<PrinubesPlatformWorkerDBContext>();
        }
        public async Task StartAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("GlobalComputePlatformThreadPool Service running.");

            //load all known compute platforms in database
            var knownPlatforms = DBContext.ComputePlatforms.Where(x => x.Enabled == true && x.state != PlatformState.Error);
            foreach (var platform in knownPlatforms)
            {
                AddPlatform(platform.Id);
            }

            //loop until we 
            while(true)
            {
                await Task.Delay(5000);
                foreach (var process in processes)
                {
                    logger.LogInformation($"GlobalComputePlatformThreadPool Service is running: {process.Item2}");
                }
            }
        }
        public void AddPlatform(Guid ComputePlatformID)
        {
            var platform = DBContext.ComputePlatforms.Single(x => x.Id.Equals(ComputePlatformID));
            CancellationTokenSource cts = new CancellationTokenSource();
            processes.Add(Tuple.Create(platform.Id, platform.Platform, cts, Task.Run(async () => {
                while(true)
                {
                    await Task.Delay(5000);
                }
                
            }, cts.Token)));
        }
        public void StopPlatform(Guid ComputePlatformID)
        {
            logger.LogInformation($"GlobalComputePlatformThreadPool Service is stopping: {ComputePlatformID}");

            if (processes.Any(x => x.Item1.Equals(ComputePlatformID)))
            {
                processes.Single(x => x.Item1.Equals(ComputePlatformID)).Item3.Cancel();
                while (!processes.Single(x => x.Item1.Equals(ComputePlatformID)).Item4.IsCompleted)
                {
                    logger.LogInformation($"GlobalComputePlatformThreadPool waiting to stop: {ComputePlatformID}");
                }
                processes.Remove(processes.Single(x => x.Item1.Equals(ComputePlatformID)));
            }
            logger.LogInformation($"GlobalComputePlatformThreadPool Service stopped: {ComputePlatformID}");
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Timed Hosted Service is stopping.");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // Do resource cleanup
        }
    }
}
