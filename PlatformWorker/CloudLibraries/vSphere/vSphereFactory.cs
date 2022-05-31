using Prinubes.Common.DatabaseModels;
using Prinubes.PlatformWorker.Datamodels;
using vspheresdk;

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
        public async Task<NetworkPlatformTestingResponseModel> TestCredentials()
        {
            try
            {
                await vsphereObject.LoginAsync();
                var systemInformation = await vsphereObject.ApplianceSubModule.SystemVersionModule.GetAsync();
                ArgumentNullException.ThrowIfNull(systemInformation.Version);
                return new NetworkPlatformTestingResponseModel() { Success = true, Message = systemInformation.Version };
            }
            catch (Exception ex)
            {
                logger.LogDebug($"Error connecting to NSX-T Endpoint {computeObject.UrlEndpoint} - {ex.Message}");
                return new NetworkPlatformTestingResponseModel() { Success = false, Message = ex.Message };
            }
        }
        public void LoadCredentials()
        {
            computeObject.Credential = DBContext.Credentials.Single(x => x.Id == computeObject.CredentialID);
        }
    }
}
