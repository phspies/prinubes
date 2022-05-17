using Microsoft.EntityFrameworkCore;
using nsxtalbsdk;
using nsxtalbsdk.Models;
using nsxtsdk;
using Prinubes.Common.DatabaseModels;
using Prinubes.PlatformWorker.Datamodels;

namespace Prinubes.PlatformWorker.CloudLibraries.NSXTALB
{
    public class NSXTALBFactory
    {
        private ILogger<NSXTALBFactory> logger;
        private LoadBalancerPlatformDatabaseModel nsxtalbobject;
        private PrinubesPlatformWorkerDBContext DBContext;
        private NSXTALBClient nsxtalbsdkObject;
        public NSXTALBFactory(LoadBalancerPlatformDatabaseModel _nsxtalbobject, PrinubesPlatformWorkerDBContext _DBContext)
        {
            nsxtalbobject = _nsxtalbobject;
            DBContext = _DBContext;
            LoadCredentials();
            nsxtalbsdkObject = new NSXTALBClient(nsxtalbobject.UrlEndpoint, nsxtalbobject.Credential.Username, nsxtalbobject.Credential.DecryptedPassword, nsxtalbobject.VertifySSLCert);

        }
        public async Task<NetworkPlatformTestingResponseModel> TestCredentials()
        {
            try
            {
                await nsxtalbsdkObject.LoginAsync();
            }
            catch (Exception ex)
            {
                logger.LogDebug($"Error connecting to NSX-T Endpoint {nsxtalbobject.UrlEndpoint} - {ex.Message}");
                return new NetworkPlatformTestingResponseModel() { Success = false, Message = ex.Message };
            }
            ClusterRuntimeType runtimerespone = await nsxtalbsdkObject.ClusterRuntimeModule.GetRuntimeAsync();
            ArgumentNullException.ThrowIfNull(runtimerespone.NodeInfo.Version);
            return new NetworkPlatformTestingResponseModel() { Success = true, Message = runtimerespone.NodeInfo.Version };
        }
        public void LoadCredentials()
        {
            nsxtalbobject.Credential = DBContext.Credentials.Single(x => x.Id == nsxtalbobject.CredentialID);
        }
    }
}
