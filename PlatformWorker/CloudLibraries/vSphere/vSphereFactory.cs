using Prinubes.Common.DatabaseModels;
using Prinubes.PlatformWorker.Datamodels;
using vspheresdk;
using vspheresdk.Appliance.Models;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere
{
    public class vSphereFactory
    {
        private ILogger logger;
        private ComputePlatformDatabaseModel computeObject;
        private PrinubesPlatformWorkerDBContext DBContext;
        private vSphereClient vsphereObject;
        public vSphereFactory(ComputePlatformDatabaseModel _computeObject, PrinubesPlatformWorkerDBContext _DBContext, ILogger _logger)
        {
            computeObject = _computeObject;
            DBContext = _DBContext;
            logger = _logger;
            LoadCredentials();
            vsphereObject = new vSphereClient(computeObject.UrlEndpoint, computeObject.Credential.Username, computeObject.Credential.DecryptedPassword, computeObject.VertifySSLCert ?? false, _timeout: 5, _retry: 3);
        }
        public async Task<ComputePlatformTestingResponseModel> TestCredentials()
        {
            try
            {
                await vsphereObject.LoginAsync();
                var systemInformation = await vsphereObject.ApplianceSubModule.SystemVersionModule.GetAsync();
                ArgumentNullException.ThrowIfNull(systemInformation.Version);
                return new ComputePlatformTestingResponseModel() { Success = true, Message = systemInformation.Version };
            }
            catch (Exception ex)
            {
                logger.LogDebug($"Error connecting to vSphere Endpoint {computeObject.UrlEndpoint} - {ex.Message}");
                return new ComputePlatformTestingResponseModel() { Success = false, Message = ex.Message };
            }
        }
        public async Task<ApplianceSystemVersionVersionStructType> Discover()
        {
            ApplianceSystemVersionVersionStructType systemInformation = null;
            try
            {
                await vsphereObject.LoginAsync();
                systemInformation = await vsphereObject.ApplianceSubModule.SystemVersionModule.GetAsync();
            }
            catch (Exception ex)
            {
                logger.LogDebug($"Error connecting to vSphere Endpoint {computeObject.UrlEndpoint} - {ex.Message}");
                throw;
            }
            ArgumentNullException.ThrowIfNull(systemInformation);
            return systemInformation;
        }
        public void LoadCredentials()
        {
            computeObject.Credential = DBContext.Credentials.Single(x => x.Id == computeObject.CredentialID);
        }
    }
}
