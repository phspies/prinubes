using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Prinubes.Common.Kafka;
using Prinubes.Common.Models;
using StackExchange.Redis;
using System;
using System.Reflection;
using UnitTesting;
using Xunit;
using Xunit.Abstractions;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestCollectionOrderer(CustomTestCollectionOrderer.TypeName, CustomTestCollectionOrderer.AssembyName)]
[assembly: TestCaseOrderer(CustomTestCaseOrderer.TypeName, CustomTestCaseOrderer.AssembyName)]

namespace UnitTesting
{
    [Order(1)]
    public class TestingSetup
    {
        [Fact, Order(0)]
        public void SetupPlatform()
        {
            GlobalVariables.identityFactory = new IdentityApplicationFactory(new ServiceSettings(_MYSQL_DATABASE: "prinubes_identity_test"));
            GlobalVariables.platformFactory = new PlatformApplicationFactory(new ServiceSettings(_MYSQL_DATABASE: "prinubes_platform_test"));
            GlobalVariables.platformWorkerFactory = new PlatformWorkerApplicationFactory(new ServiceSettings(_MYSQL_DATABASE: "prinubes_platformworker_test"));
            using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = new ServiceSettings().KAFKA_BOOTSTRAP }).Build())
            {
                foreach (var topic in adminClient.GetMetadata(TimeSpan.FromSeconds(10)).Topics)
                {
                    try
                    {
                        adminClient.DeleteTopicsAsync(new string[] { topic.Topic }).Wait();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Topic Delete Failure {e.Message}");
                    }
                }
            }

            GlobalVariables.identityFactory.Server.CreateClient();
            GlobalVariables.platformFactory.Server.CreateClient();
            GlobalVariables.platformWorkerFactory.Server.CreateClient();
            Assert.NotNull(GlobalVariables.identityFactory.WebHost);
            Assert.NotNull(GlobalVariables.platformFactory.WebHost);
            Assert.NotNull(GlobalVariables.identityFactory.DBContext);
            Assert.NotNull(GlobalVariables.platformFactory.DBContext);
            Assert.NotNull(GlobalVariables.identityFactory.Client);
            Assert.NotNull(GlobalVariables.platformFactory.Client);
        }
    }
}
