using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.DatabaseModels.PlatformEnums;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace UnitTesting
{
    [CollectionPriority(11)]
    public class Platform
    {
        static NetworkPlatformCRUDDataModel networkObject = new NetworkPlatformCRUDDataModel()
        {
            Platform = "NSX-T Network Platform",
            CredentialID = GlobalVariables.SessionNSXTCredentials.Id,
            PlatformType = NetworkPlatformType.NSXT,
            AvailabilityZone = AvailabilityZoneType.AZ1,
            UrlEndpoint = "https://nsxt01.lab.local",
            Tags = new List<TaggingModel>() { new TaggingModel() { Key = "Organization", Value = "Test Organization" }, new TaggingModel() { Key = "Cost Center", Value = "Testing" } }
        };

        static ComputePlatformCRUDDataModel computeObject = new ComputePlatformCRUDDataModel()
        {
            Platform = "vCenter Compute Platform",
            CredentialID = GlobalVariables.SessionvCenterCredentials.Id,
            PlatformType = ComputePlatformType.vSphere,
            AvailabilityZone = AvailabilityZoneType.AZ1,
            UrlEndpoint = "vcenter.lab.local",
            Tags = new List<TaggingModel>() { new TaggingModel() { Key = "Organization", Value = "Test Organization" }, new TaggingModel() { Key = "Cost Center", Value = "Testing" } }
        };
        static LoadBalancerPlatformCRUDDataModel loadbalancerObject = new LoadBalancerPlatformCRUDDataModel()
        {
            Platform = "NSX-T ALB Load Balancer Platform",
            CredentialID = GlobalVariables.SessionvCenterCredentials.Id,
            PlatformType = LoadBalancerPlatformType.NSXTALB,
            AvailabilityZone = AvailabilityZoneType.AZ1,
            UrlEndpoint = "https://nsxtalb.lab.local",
            Tags = new List<TaggingModel>() { new TaggingModel() { Key = "Organization", Value = "Test Organization" }, new TaggingModel() { Key = "Cost Center", Value = "Testing" } }
        };
        [Fact, TestPriority(1)]
        public async Task CreateNetworkPlatform()
        {
            HttpResponseMessage createResponse = await GlobalVariables.platformFactory.Client.PostAsJsonAsync($"/platform/{GlobalVariables.SessionOrganization.Id}/networkplatforms", networkObject);
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

            GlobalVariables.SessionNetworkObject = JsonConvert.DeserializeObject<NetworkPlatformDisplayDataModel>(createResponseContent);

            while (!GlobalVariables.platformWorkerFactory.DBContext.NetworkPlatforms.AsNoTracking().Any(x => x.Id == GlobalVariables.SessionNetworkObject.Id))
            {
                Task.Delay(1000).Wait();
            }

        }
        [Fact, TestPriority(2)]
        public async Task UpdateNetworkPlatform()
        {
            HttpResponseMessage retrieveResponse = await GlobalVariables.platformFactory.Client.GetAsync($"/platform/{GlobalVariables.SessionOrganization.Id}/networkplatforms/{GlobalVariables.SessionNetworkObject.Id}");
            var retrieveResponseContent = await retrieveResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, retrieveResponse.StatusCode);
            GlobalVariables.SessionNetworkObject = JsonConvert.DeserializeObject<NetworkPlatformDisplayDataModel>(retrieveResponseContent);

            GlobalVariables.SessionNetworkObject.Platform = GlobalVariables.SessionNetworkObject.Platform + " - Updated";
            GlobalVariables.SessionNetworkObject.Tags[0].Value = GlobalVariables.SessionNetworkObject.Tags[0].Value + " - updated";
            GlobalVariables.SessionNetworkObject.Tags.Add(new TaggingModel() { Key = "second test", Value = "value field" });
            HttpResponseMessage updateResponse = await GlobalVariables.platformFactory.Client.PutAsJsonAsync($"/platform/{GlobalVariables.SessionOrganization.Id}/networkplatforms/{GlobalVariables.SessionNetworkObject.Id}", GlobalVariables.SessionNetworkObject);
            var updateResponseContent = await updateResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            GlobalVariables.SessionNetworkObject = JsonConvert.DeserializeObject<NetworkPlatformDisplayDataModel>(updateResponseContent);
        }
        [Fact, TestPriority(3)]
        public async Task GetListNetworkPlatform()
        {
            var test = new object();
            HttpResponseMessage retrieveResponse = await GlobalVariables.platformFactory.Client.GetAsync($"/platform/{GlobalVariables.SessionOrganization.Id}/networkplatforms");
            var retrieveResponseContent = await retrieveResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, retrieveResponse.StatusCode);
            var networkList = JsonConvert.DeserializeObject<List<NetworkPlatformDisplayDataModel>>(retrieveResponseContent);
            Assert.NotEmpty(networkList);
        }
        [Fact, TestPriority(10)]
        public async Task CreateComputePlatform()
        {
            HttpResponseMessage createResponse = await GlobalVariables.platformFactory.Client.PostAsJsonAsync($"/platform/{GlobalVariables.SessionOrganization.Id}/computeplatforms", computeObject);
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

            GlobalVariables.SessionComputeObject = JsonConvert.DeserializeObject<ComputePlatformDisplayDataModel>(createResponseContent);
            Assert.NotNull(GlobalVariables.SessionComputeObject);
            while (!GlobalVariables.platformWorkerFactory.DBContext.ComputePlatforms.AsNoTracking().Any(x => x.Id == GlobalVariables.SessionComputeObject.Id))
            {
                Task.Delay(1000).Wait();
            }

        }
        [Fact, TestPriority(11)]
        public async Task UpdateComputePlatform()
        {
            HttpResponseMessage retrieveResponse = await GlobalVariables.platformFactory.Client.GetAsync($"/platform/{GlobalVariables.SessionOrganization.Id}/computeplatforms/{GlobalVariables.SessionComputeObject.Id}");
            var retrieveResponseContent = await retrieveResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, retrieveResponse.StatusCode);
            GlobalVariables.SessionComputeObject = JsonConvert.DeserializeObject<ComputePlatformDisplayDataModel>(retrieveResponseContent);

            GlobalVariables.SessionComputeObject.Platform = GlobalVariables.SessionComputeObject.Platform + " - Updated";
            GlobalVariables.SessionComputeObject.Tags[0].Value = GlobalVariables.SessionComputeObject.Tags[0].Value + " - updated";
            GlobalVariables.SessionComputeObject.Tags.Add(new TaggingModel() { Key = "second test", Value = "value field" });
            HttpResponseMessage updateResponse = await GlobalVariables.platformFactory.Client.PutAsJsonAsync($"/platform/{GlobalVariables.SessionOrganization.Id}/computeplatforms/{GlobalVariables.SessionComputeObject.Id}", GlobalVariables.SessionComputeObject);
            var updateResponseContent = await updateResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            GlobalVariables.SessionComputeObject = JsonConvert.DeserializeObject<ComputePlatformDisplayDataModel>(updateResponseContent);
            Assert.NotNull(GlobalVariables.SessionComputeObject);
        }
        [Fact, TestPriority(12)]
        public async Task GetListComputekPlatform()
        {
            HttpResponseMessage retrieveResponse = await GlobalVariables.platformFactory.Client.GetAsync($"/platform/{GlobalVariables.SessionOrganization.Id}/computeplatforms");
            var retrieveResponseContent = await retrieveResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, retrieveResponse.StatusCode);
            var computeList = JsonConvert.DeserializeObject<List<ComputePlatformDisplayDataModel>>(retrieveResponseContent);
            Assert.NotEmpty(computeList);
        }
        [Fact, TestPriority(13)]
        public async Task CreateLoadBalancerPlatform()
        {
            HttpResponseMessage createResponse = await GlobalVariables.platformFactory.Client.PostAsJsonAsync($"/platform/{GlobalVariables.SessionOrganization.Id}/loadbalancerplatforms", loadbalancerObject);
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

            GlobalVariables.SessionLoadBalancerObject = JsonConvert.DeserializeObject<LoadBalancerPlatformDisplayDataModel>(createResponseContent);
            Assert.NotNull(GlobalVariables.SessionLoadBalancerObject);
            while (!GlobalVariables.platformWorkerFactory.DBContext.LoadBalancerPlatforms.AsNoTracking().Any(x => x.Id == GlobalVariables.SessionLoadBalancerObject.Id))
            {
                Task.Delay(1000).Wait();
            }
        }
        [Fact, TestPriority(14)]
        public async Task UpdateLoadBalancerPlatform()
        {
            HttpResponseMessage retrieveResponse = await GlobalVariables.platformFactory.Client.GetAsync($"/platform/{GlobalVariables.SessionOrganization.Id}/loadbalancerplatforms/{GlobalVariables.SessionLoadBalancerObject.Id}");
            var retrieveResponseContent = await retrieveResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, retrieveResponse.StatusCode);
            GlobalVariables.SessionLoadBalancerObject = JsonConvert.DeserializeObject<LoadBalancerPlatformDisplayDataModel>(retrieveResponseContent);

            GlobalVariables.SessionLoadBalancerObject.Platform = GlobalVariables.SessionLoadBalancerObject.Platform + " - Updated";
            GlobalVariables.SessionLoadBalancerObject.Tags[0].Value = GlobalVariables.SessionLoadBalancerObject.Tags[0].Value + " - updated";
            GlobalVariables.SessionLoadBalancerObject.Tags.Add(new TaggingModel() { Key = "second test", Value = "value field" });
            HttpResponseMessage updateResponse = await GlobalVariables.platformFactory.Client.PutAsJsonAsync($"/platform/{GlobalVariables.SessionOrganization.Id}/loadbalancerplatforms/{GlobalVariables.SessionLoadBalancerObject.Id}", GlobalVariables.SessionLoadBalancerObject);
            var updateResponseContent = await updateResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            GlobalVariables.SessionLoadBalancerObject = JsonConvert.DeserializeObject<LoadBalancerPlatformDisplayDataModel>(updateResponseContent);
            Assert.NotNull(GlobalVariables.SessionLoadBalancerObject);
        }
        [Fact, TestPriority(15)]
        public async Task UpdateLoadBalancerWithoutTagsPlatform()
        {
            HttpResponseMessage retrieveResponse = await GlobalVariables.platformFactory.Client.GetAsync($"/platform/{GlobalVariables.SessionOrganization.Id}/loadbalancerplatforms/{GlobalVariables.SessionLoadBalancerObject.Id}");
            var retrieveResponseContent = await retrieveResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, retrieveResponse.StatusCode);
            GlobalVariables.SessionLoadBalancerObject = JsonConvert.DeserializeObject<LoadBalancerPlatformDisplayDataModel>(retrieveResponseContent);

            GlobalVariables.SessionLoadBalancerObject.Platform = GlobalVariables.SessionLoadBalancerObject.Platform + " - Updated for tags";
            GlobalVariables.SessionLoadBalancerObject.Tags = null;
            HttpResponseMessage updateResponse = await GlobalVariables.platformFactory.Client.PutAsJsonAsync($"/platform/{GlobalVariables.SessionOrganization.Id}/loadbalancerplatforms/{GlobalVariables.SessionLoadBalancerObject.Id}", GlobalVariables.SessionLoadBalancerObject);
            var updateResponseContent = await updateResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            GlobalVariables.SessionLoadBalancerObject = JsonConvert.DeserializeObject<LoadBalancerPlatformDisplayDataModel>(updateResponseContent);
            Assert.NotNull(GlobalVariables.SessionLoadBalancerObject);
        }
        [Fact, TestPriority(16)]
        public async Task GetListLoadBalancerPlatform()
        {
            HttpResponseMessage retrieveResponse = await GlobalVariables.platformFactory.Client.GetAsync($"/platform/{GlobalVariables.SessionOrganization.Id}/loadbalancerplatforms");
            var retrieveResponseContent = await retrieveResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, retrieveResponse.StatusCode);
            List<LoadBalancerPlatformDisplayDataModel> loadbalancerList = JsonConvert.DeserializeObject<List<LoadBalancerPlatformDisplayDataModel>>(retrieveResponseContent);
            var tagQuery1 = loadbalancerList.Where(x => x.Tags.Any(y => y.Key.Equals("Cost Center")));
            var tagQuery2 = loadbalancerList.SelectMany(x => x.Tags.Where(y => y.Key.Equals("Cost Center")));
            Assert.NotEmpty(tagQuery1);
            Assert.NotEmpty(tagQuery2);
            Assert.NotEmpty(loadbalancerList);
        }
        [Fact, TestPriority(50)]
        public async Task TestComputePlatformCredentials()
        {
            var testPlatform = new ComputePlatformCRUDDataModel()
            {
                Platform = "TestingPlatform",
                CredentialID = GlobalVariables.SessionvCenterCredentials.Id,
                UrlEndpoint = "vc01.lab.local",
                VertifySSLCert = false,
                PlatformType = ComputePlatformType.vSphere
            };
            HttpResponseMessage retrieveResponse = await GlobalVariables.platformFactory.Client.PostAsJsonAsync($"/platform/{GlobalVariables.SessionOrganization.Id}/computeplatforms/testCredentials", testPlatform);
            var retrieveResponseContent = await retrieveResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, retrieveResponse.StatusCode);
            ComputePlatformTestingResponseModel credentialTestResponse = JsonConvert.DeserializeObject<ComputePlatformTestingResponseModel>(retrieveResponseContent);

            Assert.NotNull(credentialTestResponse);
        }
        [Fact, TestPriority(51)]
        public async Task TestNetworkPlatformCredentials()
        {
            var testPlatform = new NetworkPlatformCRUDDataModel()
            {
                Platform = "TestingPlatform",
                CredentialID = GlobalVariables.SessionNSXTCredentials.Id,
                UrlEndpoint = "vc01.lab.local",
                VertifySSLCert = false,
                PlatformType = NetworkPlatformType.NSXT
            };
            HttpResponseMessage retrieveResponse = await GlobalVariables.platformFactory.Client.PostAsJsonAsync($"/platform/{GlobalVariables.SessionOrganization.Id}/networkplatforms/testCredentials", testPlatform);
            var retrieveResponseContent = await retrieveResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, retrieveResponse.StatusCode);
            NetworkPlatformTestingResponseModel testResponse = JsonConvert.DeserializeObject<NetworkPlatformTestingResponseModel>(retrieveResponseContent);

            Assert.NotNull(testResponse);
        }
        [Fact, TestPriority(52)]
        public async Task TestLoadBalancerPlatformCredentials()
        {
            var testPlatform = new LoadBalancerPlatformCRUDDataModel()
            {
                Platform = "TestingPlatform",
                CredentialID = GlobalVariables.SessionNSXALBCredentials.Id,
                UrlEndpoint = "vc01.lab.local",
                VertifySSLCert = false,
                PlatformType = LoadBalancerPlatformType.NSXTALB
            };
            HttpResponseMessage retrieveResponse = await GlobalVariables.platformFactory.Client.PostAsJsonAsync($"/platform/{GlobalVariables.SessionOrganization.Id}/loadbalancerplatforms/testCredentials", testPlatform);
            var retrieveResponseContent = await retrieveResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, retrieveResponse.StatusCode);
            LoadBalancerPlatformTestingResponseModel testResponse = JsonConvert.DeserializeObject<LoadBalancerPlatformTestingResponseModel>(retrieveResponseContent);

            Assert.NotNull(testResponse);
        }
    }
}