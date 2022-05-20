using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Prinubes.Common.Models;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace UnitTesting
{
    [CollectionPriority(99)]
    public class TestingCleanup
    {
        [Fact, TestPriority(1)]
        public async Task WaitForChannels()
        {
            await Task.Delay(1200000);
        }
        [Fact, TestPriority(47)]
        public async Task DeleteALBPlatform()
        {
            HttpResponseMessage retrieveResponse = await GlobalVariables.platformFactory.Client.DeleteAsync($"/platform/{GlobalVariables.SessionOrganization.Id}/loadbalancerplatforms/{GlobalVariables.SessionLoadBalancerObject.Id}");
            Assert.Equal(HttpStatusCode.OK, retrieveResponse.StatusCode);
            while (GlobalVariables.platformFactory.DBContext.LoadBalancerPlatforms.AsNoTracking().Any(x => x.Id == GlobalVariables.SessionLoadBalancerObject.Id))
            {
                Task.Delay(1000).Wait();
            }
        }
        [Fact, TestPriority(48)]
        public async Task DeleteComputePlatform()
        {
            HttpResponseMessage retrieveResponse = await GlobalVariables.platformFactory.Client.DeleteAsync($"/platform/{GlobalVariables.SessionOrganization.Id}/computeplatforms/{GlobalVariables.SessionComputeObject.Id}");
            Assert.Equal(HttpStatusCode.OK, retrieveResponse.StatusCode);
            while (GlobalVariables.platformFactory.DBContext.ComputePlatforms.AsNoTracking().Any(x => x.Id == GlobalVariables.SessionComputeObject.Id))
            {
                Task.Delay(1000).Wait();
            }
        }
        [Fact, TestPriority(49)]
        public async Task DeleteNetworkPlatform()
        {
            HttpResponseMessage retrieveResponse = await GlobalVariables.platformFactory.Client.DeleteAsync($"/platform/{GlobalVariables.SessionOrganization.Id}/networkplatforms/{GlobalVariables.SessionNetworkObject.Id}");
            Assert.Equal(HttpStatusCode.OK, retrieveResponse.StatusCode);
            while (GlobalVariables.platformFactory.DBContext.NetworkPlatforms.AsNoTracking().Any(x => x.Id == GlobalVariables.SessionNetworkObject.Id))
            {
                Task.Delay(1000).Wait();
            }
        }
        [Fact, TestPriority(50)]
        public async Task DeleteNSXALBCredentials()
        {
            HttpResponseMessage getResponse = await GlobalVariables.identityFactory.Client.DeleteAsync($"/identity/{GlobalVariables.SessionOrganization.Id}/credentials/{GlobalVariables.SessionNSXALBCredentials.Id}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            while (GlobalVariables.platformFactory.DBContext.Credentials.AsNoTracking().Any(x => x.Id == GlobalVariables.SessionNSXALBCredentials.Id))
            {
                Task.Delay(1000).Wait();
            }
        }
        [Fact, TestPriority(51)]
        public async Task DeleteNSXTCredentials()
        {
            HttpResponseMessage getResponse = await GlobalVariables.identityFactory.Client.DeleteAsync($"/identity/{GlobalVariables.SessionOrganization.Id}/credentials/{GlobalVariables.SessionNSXTCredentials.Id}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            while (GlobalVariables.platformFactory.DBContext.Credentials.AsNoTracking().Any(x => x.Id == GlobalVariables.SessionNSXTCredentials.Id))
            {
                Task.Delay(1000).Wait();
            }
        }
        [Fact, TestPriority(52)]
        public async Task DeletevCenterCredentials()
        {
            HttpResponseMessage getResponse = await GlobalVariables.identityFactory.Client.DeleteAsync($"/identity/{GlobalVariables.SessionOrganization.Id}/credentials/{GlobalVariables.SessionvCenterCredentials.Id}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            while (GlobalVariables.platformFactory.DBContext.Credentials.AsNoTracking().Any(x => x.Id == GlobalVariables.SessionvCenterCredentials.Id))
            {
                Task.Delay(1000).Wait();
            }
        }
        [Fact, TestPriority(53)]
        public async Task DetachTestFromGroup()
        {
            HttpResponseMessage groupAttachResponse = await GlobalVariables.identityFactory.Client.PutAsJsonAsync($"/identity/{GlobalVariables.SessionOrganization.Id}/groups/{GlobalVariables.SessionGroup.Id}/detachUser/{GlobalVariables.SessionToken.id}", new Object());
            Assert.Equal(HttpStatusCode.OK, groupAttachResponse.StatusCode);
            while (GlobalVariables.platformFactory.DBContext.Groups.AsNoTracking().Include(x => x.Users).Single(x => x.Id == GlobalVariables.SessionGroup.Id).Users.Any(x => x.Id == GlobalVariables.SessionToken.id))
            {
                Task.Delay(1000).Wait();
            }
        }
        [Fact, TestPriority(54)]
        public async Task DeleteTestGroup()
        {
            HttpResponseMessage groupAttachResponse = await GlobalVariables.identityFactory.Client.DeleteAsync($"/identity/{GlobalVariables.SessionOrganization.Id}/groups/{GlobalVariables.SessionGroup.Id}");
            Assert.Equal(HttpStatusCode.OK, groupAttachResponse.StatusCode);
            while (GlobalVariables.platformFactory.DBContext.Groups.AsNoTracking().Any(x => x.Id == GlobalVariables.SessionGroup.Id))
            {
                Task.Delay(1000).Wait();
            }
        }
        [Fact, TestPriority(55)]
        public async Task DeleteTestOrganization()
        {
            HttpResponseMessage groupAttachResponse = await GlobalVariables.identityFactory.Client.DeleteAsync($"/identity/organizations/{GlobalVariables.SessionOrganization.Id}");
            Assert.Equal(HttpStatusCode.OK, groupAttachResponse.StatusCode);
            while (GlobalVariables.platformFactory.DBContext.Organizations.AsNoTracking().Any(x => x.Id == GlobalVariables.SessionOrganization.Id))
            {
                Task.Delay(1000).Wait();
            }
        }
        [Fact, TestPriority(999)]
        public void Cleanup()
        {
            //flush redis cache
            //ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(new ConfigurationOptions { EndPoints = { $"{new ServiceSettings().REDIS_CACHE_HOST}:{new ServiceSettings().REDIS_CACHE_PORT}" } });
            //redis.GetDatabase().Execute("FLUSHALL;");


            GlobalVariables.identityFactory.WebHost.StopAsync().Wait();
            GlobalVariables.platformFactory.WebHost.StopAsync().Wait();
            GlobalVariables.platformWorkerFactory.WebHost.StopAsync().Wait();
            GlobalVariables.identityFactory.DBContext.Database.EnsureDeleted();
            GlobalVariables.platformFactory.DBContext.Database.EnsureDeleted();
            GlobalVariables.platformWorkerFactory.DBContext.Database.EnsureDeleted();
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

        }
    }
}
