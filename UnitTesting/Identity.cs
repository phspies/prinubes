using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Datamodels;
using Prinubes.Common.Helpers;
using Prinubes.Common.Models;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UnitTesting.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace UnitTesting
{
    [Order(10)]
    public class Identity 
    {
  


        static UserCRUDDataModel userObject = new UserCRUDDataModel()
        {
            Firstname = "Test",
            Lastname = "Test",
            EmailAddress = "test@pninubes.com",
            Password = "password"
        };
 
    
        [Fact, Order(1)]
        public async Task RegisterTestUser1()
        {
            HttpResponseMessage registerResponse = await GlobalVariables.identityFactory.Client.PostAsJsonAsync("/identity/users/register", userObject);
            Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
            var authenticateObject = new AuthenticateModel()
            {
                Username = userObject.EmailAddress,
                Password = userObject.Password
            };
            HttpResponseMessage authenticateResponse = await GlobalVariables.identityFactory.Client.PostAsJsonAsync("/identity/users/authenticate", authenticateObject);
            Assert.Equal(HttpStatusCode.OK, authenticateResponse.StatusCode);

            //set client header to auth token
            GlobalVariables.SessionToken = JsonConvert.DeserializeObject<AuthenticateResponse>(await authenticateResponse.Content.ReadAsStringAsync());
            GlobalVariables.identityFactory.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalVariables.SessionToken.token);
            GlobalVariables.platformFactory.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalVariables.SessionToken.token);
        }
        [Fact, Order(2)]
        public async Task CreateTestOrganization()
        {
            OrganizationCRUDDataModel organizationDataModel = new OrganizationCRUDDataModel()
            {
                Organization = "unitTestOrganization"
            };
            HttpResponseMessage organizationResponse = await GlobalVariables.identityFactory.Client.PostAsJsonAsync("/identity/organizations", organizationDataModel);
            Assert.Equal(HttpStatusCode.OK, organizationResponse.StatusCode);
            var orgContext = await organizationResponse.Content.ReadAsStringAsync();
            GlobalVariables.SessionOrganization = JsonConvert.DeserializeObject<OrganizationDisplayDataModel>(orgContext);
            Assert.NotNull(GlobalVariables.SessionOrganization);
            while (true)
            {
                if (!GlobalVariables.platformFactory.DBContext.Organizations.AsNoTracking().Any(x => x.Id == GlobalVariables.SessionOrganization.Id))
                {
                    Task.Delay(1000).Wait();
                }
                else
                {
                    break;
                }
            }
        }
        [Fact, Order(3)]
        public async Task CreateTestGroup()
        {
            //create test group
            GroupCRUDDataModel groupObject = new GroupCRUDDataModel()
            {
                Group = "testgroup01"
            };
            HttpResponseMessage groupResponse = await GlobalVariables.identityFactory.Client.PostAsJsonAsync($"/identity/{GlobalVariables.SessionOrganization.Id}/groups", groupObject);
            var groupContents = await groupResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, groupResponse.StatusCode);
            GlobalVariables.SessionGroup = JsonConvert.DeserializeObject<GroupDisplayDataModel>(await groupResponse.Content.ReadAsStringAsync());
            Assert.NotNull(GlobalVariables.SessionGroup);
            while (true)
            {
                if (!GlobalVariables.platformFactory.DBContext.Groups.AsNoTracking().Any(x => x.Id == GlobalVariables.SessionGroup.Id))
                {
                    Task.Delay(1000).Wait();
                }
                else
                {
                    break;
                }
            }
        }
        [Fact, Order(4)]
        public async Task AttachTestToGroup()
        {

            HttpResponseMessage groupAttachResponse = await GlobalVariables.identityFactory.Client.PutAsJsonAsync($"/identity/{GlobalVariables.SessionOrganization.Id}/groups/{GlobalVariables.SessionGroup.Id}/attachUser/{GlobalVariables.SessionToken.id}", new Object());
            var groupAttachContents = await groupAttachResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, groupAttachResponse.StatusCode);
            while (true)
            {
                if (!GlobalVariables.platformFactory.DBContext.Groups.AsNoTracking().Include(x => x.Users).Single(x => x.Id == GlobalVariables.SessionGroup.Id).Users.Any(x => x.Id == GlobalVariables.SessionToken.id))
                {
                    Task.Delay(1000).Wait();
                }
                else
                {
                    break;
                }
            }
        }
        [Fact, Order(5)]
        public async Task CreatevCenterCredentials()
        {

            CredentialCRUDDataModel vCenterCredentialsObject = new CredentialCRUDDataModel()
            {
                Credential = "vCenter Credentials",
                Username = "administrator@vsphere.local",
                Password = "c0mp2q",
            };
            HttpResponseMessage vcenterCredentialsResponse = await GlobalVariables.identityFactory.Client.PostAsJsonAsync($"/identity/{GlobalVariables.SessionOrganization.Id}/credentials", vCenterCredentialsObject);
            Assert.Equal(HttpStatusCode.OK, vcenterCredentialsResponse.StatusCode);

            var vcenterCredentialsContents = await vcenterCredentialsResponse.Content.ReadAsStringAsync();
            GlobalVariables.SessionvCenterCredentials = JsonConvert.DeserializeObject<CredentialDisplayDataModel>(vcenterCredentialsContents);
            Assert.NotNull(GlobalVariables.SessionvCenterCredentials);
            while (true)
            {
                if (!GlobalVariables.platformFactory.DBContext.Credentials.AsNoTracking().Any(x => x.Id == GlobalVariables.SessionvCenterCredentials.Id))
                {
                    Task.Delay(1000).Wait();
                    
                }
                else
                {
                    break;
                }
            }
        }
        [Fact, Order(6)]
        public async Task CreateNSXTCredentials5()
        {
            CredentialCRUDDataModel vNSXTCredentialsObject = new CredentialCRUDDataModel()
            {
                Credential = "NSX-T Credentials",
                Username = "admin",
                Password = "c0mp2q",
            };
            HttpResponseMessage vnsxtCredentialsResponse = await GlobalVariables.identityFactory.Client.PostAsJsonAsync($"/identity/{GlobalVariables.SessionOrganization.Id}/credentials", vNSXTCredentialsObject);
            Assert.Equal(HttpStatusCode.OK, vnsxtCredentialsResponse.StatusCode);

            var vnsxtCredentialsContents = await vnsxtCredentialsResponse.Content.ReadAsStringAsync();
            GlobalVariables.SessionNSXTCredentials = JsonConvert.DeserializeObject<CredentialDisplayDataModel>(vnsxtCredentialsContents);
            Assert.NotNull(GlobalVariables.SessionNSXTCredentials);
            while (true)
            {
                if (!GlobalVariables.platformFactory.DBContext.Credentials.AsNoTracking().Any(x => x.Id == GlobalVariables.SessionNSXTCredentials.Id))
                {
                    Task.Delay(1000).Wait();
                }
                else
                {
                    break;
                }
            }
        }
        [Fact, Order(7)]
        public async Task CreateNSXALBCredentials6()
        {
            CredentialCRUDDataModel vNSXALBCredentialsObject = new CredentialCRUDDataModel()
            {
                Credential = "NSXALB Credentials",
                Username = "admin",
                Password = "c0mp2q",
            };
            HttpResponseMessage vnsxalbCredentialsResponse = await GlobalVariables.identityFactory.Client.PostAsJsonAsync($"/identity/{GlobalVariables.SessionOrganization.Id}/credentials", vNSXALBCredentialsObject);
            Assert.Equal(HttpStatusCode.OK, vnsxalbCredentialsResponse.StatusCode);

            var vnsxalbCredentialsContents = await vnsxalbCredentialsResponse.Content.ReadAsStringAsync();
            GlobalVariables.SessionNSXALBCredentials = JsonConvert.DeserializeObject<CredentialDisplayDataModel>(vnsxalbCredentialsContents);
            Assert.NotNull(GlobalVariables.SessionNSXALBCredentials);
            while (true)
            {
                if (!GlobalVariables.platformFactory.DBContext.Credentials.AsNoTracking().Any(x => x.Id == GlobalVariables.SessionNSXALBCredentials.Id))
                {
                    Task.Delay(1000).Wait();
                }
                else
                {
                    break;
                }
            }
        }
        [Fact, Order(8)]
        public async Task UpdateTestUser()
        {
            userObject.Firstname = userObject.Firstname + "UpdatedTest";
            HttpResponseMessage updateGetResponse = await GlobalVariables.identityFactory.Client.GetAsync($"/identity/users/{GlobalVariables.SessionToken.id}");
            Assert.Equal(HttpStatusCode.OK, updateGetResponse.StatusCode);

            var UpdateTestUserContents = await updateGetResponse.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<UserDisplayDataModel>(UpdateTestUserContents);

            Assert.NotNull(user);
            UserCRUDDataModel crudUser = new UserCRUDDataModel();
            PropertyCopier.Populate(user, crudUser);
            crudUser.Firstname = userObject.Firstname + "-test";
            crudUser.Lastname = userObject.Lastname + "-test";
            crudUser.Password = "Password-test";

            HttpResponseMessage updatePutResponse = await GlobalVariables.identityFactory.Client.PutAsJsonAsync($"/identity/users/{user.Id}", crudUser);
            Assert.Equal(HttpStatusCode.OK, updatePutResponse.StatusCode);
            while (true)
            {
                if (!GlobalVariables.platformFactory.DBContext.Users.AsNoTracking().Any(x => x.Id == GlobalVariables.SessionToken.id && x.Firstname == crudUser.Firstname))
                {
                    Task.Delay(1000).Wait();
                }
                else
                {
                    break;
                }
            }
        }
        [Fact, Order(9)]
        public async Task UpdateOrganization()
        {
            HttpResponseMessage getResponse = await GlobalVariables.identityFactory.Client.GetAsync($"/identity/organizations/{GlobalVariables.SessionOrganization.Id}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var getContents = await getResponse.Content.ReadAsStringAsync();
            var updateObject = JsonConvert.DeserializeObject<OrganizationCRUDDataModel>(getContents);

            Assert.NotNull(updateObject);
            updateObject.Organization = "unitTestOrganization - Updated";

            HttpResponseMessage updateResponse = await GlobalVariables.identityFactory.Client.PutAsJsonAsync($"/identity/organizations/{GlobalVariables.SessionOrganization.Id}", updateObject);
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            while (true)
            {
                if (!GlobalVariables.platformFactory.DBContext.Organizations.AsNoTracking().Any(x => x.Id == GlobalVariables.SessionOrganization.Id && x.Organization == updateObject.Organization))
                {
                    Task.Delay(1000).Wait();
                }
                else
                {
                    break;
                }
            }
        }
        [Fact, Order(9)]
        public async Task UpdateGroup()
        {
            HttpResponseMessage getResponse = await GlobalVariables.identityFactory.Client.GetAsync($"/identity/{GlobalVariables.SessionOrganization.Id}/groups/{GlobalVariables.SessionGroup.Id}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var getContents = await getResponse.Content.ReadAsStringAsync();
            var updateObject = JsonConvert.DeserializeObject<GroupCRUDDataModel>(getContents);

            Assert.NotNull(updateObject);
            updateObject.Group = updateObject.Group + " - Updated";

            HttpResponseMessage updateResponse = await GlobalVariables.identityFactory.Client.PutAsJsonAsync($"/identity/{GlobalVariables.SessionOrganization.Id}/groups/{GlobalVariables.SessionGroup.Id}", updateObject);
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

            HttpResponseMessage get2Response = await GlobalVariables.identityFactory.Client.GetAsync($"/identity/{GlobalVariables.SessionOrganization.Id}/groups/{GlobalVariables.SessionGroup.Id}");
            Assert.Equal(HttpStatusCode.OK, get2Response.StatusCode);
            var get2Contents = await get2Response.Content.ReadAsStringAsync();
            var update2Object = JsonConvert.DeserializeObject<GroupCRUDDataModel>(get2Contents);

            Assert.Equal(updateObject.Group, update2Object.Group);
            while (true)
            {
                if (!GlobalVariables.platformFactory.DBContext.Groups.AsNoTracking().Any(x => x.Id == GlobalVariables.SessionGroup.Id && x.Group == updateObject.Group))
                {
                    Task.Delay(1000).Wait();
                }
                else
                {
                    break;
                }
            }
        }
        [Fact, Order(9)]
        public async Task UpdatevCenterCredentials()
        {
            HttpResponseMessage getResponse = await GlobalVariables.identityFactory.Client.GetAsync($"/identity/{GlobalVariables.SessionOrganization.Id}/credentials/{GlobalVariables.SessionvCenterCredentials.Id}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var getContents = await getResponse.Content.ReadAsStringAsync();
            var updateObject = JsonConvert.DeserializeObject<CredentialCRUDDataModel>(getContents);

            Assert.NotNull(updateObject);
            updateObject.Credential = updateObject.Credential + " - Updated";

            HttpResponseMessage updateResponse = await GlobalVariables.identityFactory.Client.PutAsJsonAsync($"/identity/{GlobalVariables.SessionOrganization.Id}/credentials/{GlobalVariables.SessionvCenterCredentials.Id}", updateObject);
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

            HttpResponseMessage get2Response = await GlobalVariables.identityFactory.Client.GetAsync($"/identity/{GlobalVariables.SessionOrganization.Id}/credentials/{GlobalVariables.SessionvCenterCredentials.Id}");
            Assert.Equal(HttpStatusCode.OK, get2Response.StatusCode);
            var get2Contents = await get2Response.Content.ReadAsStringAsync();
            var update2Object = JsonConvert.DeserializeObject<CredentialCRUDDataModel>(get2Contents);

            Assert.Equal(updateObject.Credential, update2Object.Credential);
            while (true)
            {
                if (!GlobalVariables.platformFactory.DBContext.Credentials.AsNoTracking().Any(x => x.Id == GlobalVariables.SessionvCenterCredentials.Id && x.Credential == updateObject.Credential))
                {
                    Task.Delay(1000).Wait();
                }
                else
                {
                    break;
                }
            }
        }

        [Fact, Order(10)]
        public async Task UpdateNSXTCredentials()
        {
            HttpResponseMessage getResponse = await GlobalVariables.identityFactory.Client.GetAsync($"/identity/{GlobalVariables.SessionOrganization.Id}/credentials/{GlobalVariables.SessionNSXTCredentials.Id}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var getContents = await getResponse.Content.ReadAsStringAsync();
            var updateObject = JsonConvert.DeserializeObject<CredentialCRUDDataModel>(getContents);

            Assert.NotNull(updateObject);
            updateObject.Credential = updateObject.Credential + " - Updated";

            HttpResponseMessage updateResponse = await GlobalVariables.identityFactory.Client.PutAsJsonAsync($"/identity/{GlobalVariables.SessionOrganization.Id}/credentials/{GlobalVariables.SessionNSXTCredentials.Id}", updateObject);
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

            HttpResponseMessage get2Response = await GlobalVariables.identityFactory.Client.GetAsync($"/identity/{GlobalVariables.SessionOrganization.Id}/credentials/{GlobalVariables.SessionNSXTCredentials.Id}");
            Assert.Equal(HttpStatusCode.OK, get2Response.StatusCode);
            var get2Contents = await get2Response.Content.ReadAsStringAsync();
            var update2Object = JsonConvert.DeserializeObject<CredentialCRUDDataModel>(get2Contents);

            Assert.Equal(updateObject.Credential, update2Object.Credential);
            while (true)
            {
                if (!GlobalVariables.platformFactory.DBContext.Credentials.AsNoTracking().Any(x => x.Id == GlobalVariables.SessionNSXTCredentials.Id && x.Credential == updateObject.Credential))
                {
                    Task.Delay(1000).Wait();
                }
                else
                {
                    break;
                }
            }
        }
        [Fact, Order(11)]
        public async Task UpdateNSXALBCredentials()
        {
            HttpResponseMessage getResponse = await GlobalVariables.identityFactory.Client.GetAsync($"/identity/{GlobalVariables.SessionOrganization.Id}/credentials/{GlobalVariables.SessionNSXALBCredentials.Id}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var getContents = await getResponse.Content.ReadAsStringAsync();
            var updateObject = JsonConvert.DeserializeObject<CredentialCRUDDataModel>(getContents);

            Assert.NotNull(updateObject);
            updateObject.Credential = updateObject.Credential + " - Updated";

            HttpResponseMessage updateResponse = await GlobalVariables.identityFactory.Client.PutAsJsonAsync($"/identity/{GlobalVariables.SessionOrganization.Id}/credentials/{GlobalVariables.SessionNSXALBCredentials.Id}", updateObject);
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

            HttpResponseMessage get2Response = await GlobalVariables.identityFactory.Client.GetAsync($"/identity/{GlobalVariables.SessionOrganization.Id}/credentials/{GlobalVariables.SessionNSXALBCredentials.Id}");
            Assert.Equal(HttpStatusCode.OK, get2Response.StatusCode);
            var get2Contents = await get2Response.Content.ReadAsStringAsync();
            var update2Object = JsonConvert.DeserializeObject<CredentialCRUDDataModel>(get2Contents);

            Assert.Equal(updateObject.Credential, update2Object.Credential);
            while (true)
            {
                if (!GlobalVariables.platformFactory.DBContext.Credentials.AsNoTracking().Any(x => x.Id == GlobalVariables.SessionNSXALBCredentials.Id && x.Credential == updateObject.Credential))
                {
                    Task.Delay(1000).Wait();
                }
                else
                {
                    break;
                }
            }
        }
  
    }
}