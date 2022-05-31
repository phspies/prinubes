using Microsoft.EntityFrameworkCore;
using nsxtsdk;
using nsxtsdk.ManagerModels;
using Prinubes.Common.DatabaseModels;
using Prinubes.PlatformWorker.Datamodels;

namespace Prinubes.PlatformWorker.CloudLibraries.NSXT
{
    public class NSXTFactory
    {
        private ILogger logger;
        private NetworkPlatformDatabaseModel nsxtobject;
        private PrinubesPlatformWorkerDBContext DBContext;
        private NSXTClient nsxtsdkObject;
        public NSXTFactory(NetworkPlatformDatabaseModel _nsxtobject, PrinubesPlatformWorkerDBContext _DBContext, ILogger _logger)
        {
            nsxtobject = _nsxtobject;
            DBContext = _DBContext;
            logger = _logger;
            LoadCredentials();
            nsxtsdkObject = new NSXTClient(nsxtobject.UrlEndpoint, nsxtobject.Credential.Username, nsxtobject.Credential.DecryptedPassword, _nsxtobject.VertifySSLCert, _timeout: 5, _retry: 3);

        }
        public async Task<NetworkPlatformTestingResponseModel> TestCredentials()
        {
            try
            {
                await nsxtsdkObject.LoginAsync();
                var managementPlaneconfig = (await nsxtsdkObject.ManagerEngine.ClusterManagementModule.ReadClusterNodesAggregateStatus());
                ArgumentNullException.ThrowIfNull(managementPlaneconfig?.ControllerCluster[0]?.NodeStatus?.Version);
                return new NetworkPlatformTestingResponseModel() { Success = true, Message = managementPlaneconfig.ControllerCluster[0].NodeStatus.Version };

            }
            catch (Exception ex)
            {
                logger.LogDebug($"Error connecting to NSX-T Endpoint {nsxtobject.UrlEndpoint} - {ex.Message}");
                return new NetworkPlatformTestingResponseModel() { Success = false, Message = ex.Message };
            }
        }

        public void LoadCredentials()
        {
            nsxtobject.Credential = DBContext.Credentials.Single(x => x.Id == nsxtobject.CredentialID);
        }
    }
}
