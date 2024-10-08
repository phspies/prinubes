﻿using Confluent.Kafka;
using Prinubes.Common.Models;
using System;
using UnitTesting;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestCollectionOrderer(UnitTestCollectionOrderer.TypeName, UnitTestCollectionOrderer.AssembyName)]
[assembly: TestCaseOrderer(UnitTestCaseOrderer.TypeName, UnitTestCaseOrderer.AssembyName)]

namespace UnitTesting
{
    [CollectionPriority(1)]
    public class TestingSetup
    {
        [Fact, TestPriority(1)]
        public void Setup()
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
