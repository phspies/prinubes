using Microsoft.EntityFrameworkCore;
using nsxtalbsdk;
using nsxtalbsdk.Models;
using nsxtsdk;
using Prinubes.Common.DatabaseModels;
using Prinubes.PlatformWorker.Datamodels;
using Prinubes.PlatformWorker.CloudLibraries.NSXTALB;
using vspheresdk;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere
{
    public class vSphereFactory
    {
        private ILogger<NSXTALBFactory> logger;
        private ComputePlatformDatabaseModel computeObject;
        private PrinubesPlatformWorkerDBContext DBContext;
        private vSphereClient vsphereObject;
        public vSphereFactory(ComputePlatformDatabaseModel _computeObject, PrinubesPlatformWorkerDBContext _DBContext)
        {
            computeObject = _computeObject;
            DBContext = _DBContext;
            LoadCredentials();
            vsphereObject = new vSphereClient(computeObject.UrlEndpoint, computeObject.Credential.Username, computeObject.Credential.DecryptedPassword, computeObject.VertifySSLCert ?? false);

        }
        public async Task<NetworkPlatformTestingResponseModel> TestCredentials()
        {
            try
            {
                await vsphereObject.LoginAsync();
            }
            catch (Exception ex)
            {
                logger.LogDebug($"Error connecting to NSX-T Endpoint {computeObject.UrlEndpoint} - {ex.Message}");
                return new NetworkPlatformTestingResponseModel() { Success = false, Message = ex.Message };
            }
            var systemInformation = await vsphereObject.ApplianceSubModule.SystemVersionModule.GetAsync();
            ArgumentNullException.ThrowIfNull(systemInformation.Version);
            return new NetworkPlatformTestingResponseModel() { Success = true, Message = systemInformation.Version };
        }
        public void LoadCredentials()
        {
            computeObject.Credential = DBContext.Credentials.Single(x => x.Id == computeObject.CredentialID);
        }
    }
}
