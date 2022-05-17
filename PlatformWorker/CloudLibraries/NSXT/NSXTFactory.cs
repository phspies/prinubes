using Microsoft.EntityFrameworkCore;
using nsxtsdk;
using Prinubes.Common.DatabaseModels;
using Prinubes.PlatformWorker.Datamodels;

namespace Prinubes.PlatformWorker.CloudLibraries.NSXT
{
    public class NSXTFactory
    {
        private ILogger<NSXTFactory> logger;
        private NetworkPlatformDatabaseModel nsxtobject;
        private PrinubesPlatformWorkerDBContext DBContext;
        private NSXTClient nsxtsdkObject;
        public NSXTFactory(NetworkPlatformDatabaseModel _nsxtobject, PrinubesPlatformWorkerDBContext _DBContext)
        {
            nsxtobject = _nsxtobject;
            DBContext = _DBContext;
            LoadCredentials();
            nsxtsdkObject = new NSXTClient(nsxtobject.UrlEndpoint, nsxtobject.Credential.Username, nsxtobject.Credential.DecryptedPassword, _nsxtobject.VertifySSLCert);

        }
        public async Task<NetworkPlatformTestingResponseModel> TestCredentials()
        {
            try
            {
                await nsxtsdkObject.LoginAsync();
            }
            catch (Exception ex)
            {
                logger.LogDebug($"Error connecting to NSX-T Endpoint {nsxtobject.UrlEndpoint} - {ex.Message}");
                return new NetworkPlatformTestingResponseModel() { Success = false, Message = ex.Message };
            }
            var managementPlaneconfig = (await nsxtsdkObject.ManagerEngine.ClusterManagementModule.ReadClusterNodesAggregateStatus());
            ArgumentNullException.ThrowIfNull(managementPlaneconfig?.ControllerCluster[0]?.NodeStatus?.Version);
            return new NetworkPlatformTestingResponseModel() { Success = true, Message = managementPlaneconfig?.ControllerCluster[0]?.NodeStatus?.Version };
        }

        public void LoadCredentials()
        {
            nsxtobject.Credential = DBContext.Credentials.Single(x => x.Id == nsxtobject.CredentialID);
        }
    }
}
