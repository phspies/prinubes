using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Helpers;
using Prinubes.Common.Kafka.Producer;

namespace Prinubes.Platforms.Datamodels.Domain
{
    public class RoutePathDOA : IDisposable
    {
        private PrinubesPlatformDBContext dbContext;
        private ILogger logger;
        private IMessageProducer kafkaProducer;

        public RoutePathDOA(IServiceProvider serviceProvider)
        {
            dbContext = serviceProvider.GetRequiredService<PrinubesPlatformDBContext>();
            logger = serviceProvider.GetRequiredService<ILogger<UserDOA>>();
            kafkaProducer = serviceProvider.GetRequiredService<IMessageProducer>();
        }
        public async Task SyncronizeAsync(IReadOnlyList<ActionDescriptor> _descriptors)
        {

            logger.LogDebug($"Processing {_descriptors.Count()} routes for RoutePaths");
            foreach (ControllerActionDescriptor route in _descriptors.Where(x => x.GetType() == typeof(ControllerActionDescriptor)))
            {
                //only known actions in routemap
                if (route.EndpointMetadata.Any(x => x.GetType() == typeof(PrinubesDescriptionAttribute)))
                {
                    logger.LogDebug($"Processing {route.Id} : {route.DisplayName}");
                    string routePathUnique = $"{route.ActionName.ToLower()}:{route.AttributeRouteInfo.Template}";
                    PrinubesDescriptionAttribute name = (PrinubesDescriptionAttribute)route.EndpointMetadata.First(x => x.GetType() == typeof(PrinubesDescriptionAttribute));
                    RoutePathDatabaseModel routePath = new RoutePathDatabaseModel();
                    bool newRoute = false;
                    if (dbContext.RoutePaths.Any(x => x.RoutePathUnique == routePathUnique))
                    {
                        routePath = await dbContext.RoutePaths.SingleAsync(x => x.RoutePathUnique == routePathUnique);
                    }
                    else
                    {
                        routePath.Id = Guid.NewGuid();
                        newRoute = true;
                    }

                    routePath.FriendlyName = name.FriendlyName[0];
                    routePath.DisplayName = route.DisplayName;
                    routePath.MicroService = routePath.DisplayName.Split('.')[0];
                    routePath.Controller = route.ControllerName;
                    routePath.MethodName = route.ActionName;
                    routePath.RouteTemplate = route.AttributeRouteInfo.Template;
                    routePath.RoutePathUnique = routePathUnique;

                    if (dbContext.Entry(routePath).State == EntityState.Modified)
                    {
                        using (var transaction = dbContext.Database.BeginTransaction())
                        {
                            await dbContext.SaveChangesAsync();
                            KafkaMessage.SubmitKafkaMessageAync(
                                  new RoutePathKafkaMessage()
                                  {
                                      Action = ActionEnum.update,
                                      RoutePathID = routePath.Id,
                                      RoutePath = routePath
                                  },
                                  logger,
                                  kafkaProducer);
                            await transaction.CommitAsync();
                        }
                    }
                    else if (newRoute)
                    {
                        using (var transaction = dbContext.Database.BeginTransaction())
                        {
                            dbContext.RoutePaths.Add(routePath);
                            dbContext.SaveChanges();
                            KafkaMessage.SubmitKafkaMessageAync(
                                  new RoutePathKafkaMessage()
                                  {
                                      Action = ActionEnum.create,
                                      RoutePathID = routePath.Id,
                                      RoutePath = routePath
                                  },
                                  logger,
                                  kafkaProducer);
                            transaction.Commit();
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            // ((IDisposable)dbContext).Dispose();
        }
    }
}
