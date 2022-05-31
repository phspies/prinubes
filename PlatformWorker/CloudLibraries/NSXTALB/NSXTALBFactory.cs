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
        private ILogger logger;
        private LoadBalancerPlatformDatabaseModel nsxtalbobject;
        private PrinubesPlatformWorkerDBContext DBContext;
        private NSXTALBClient nsxtalbsdkObject;
        public NSXTALBFactory(LoadBalancerPlatformDatabaseModel _nsxtalbobject, PrinubesPlatformWorkerDBContext _DBContext, ILogger _logger)
        {
            nsxtalbobject = _nsxtalbobject;
            DBContext = _DBContext;
            logger = _logger;
            LoadCredentials();
            nsxtalbsdkObject = new NSXTALBClient(nsxtalbobject.UrlEndpoint, nsxtalbobject.Credential.Username, nsxtalbobject.Credential.DecryptedPassword, nsxtalbobject.VertifySSLCert, _timeout: 5, _retry: 3);

        }
        public async Task<LoadBalancerPlatformTestingResponseModel> TestCredentials()
        {
            try
            {
                await nsxtalbsdkObject.LoginAsync();
                ClusterRuntimeType runtimerespone = await nsxtalbsdkObject.ClusterRuntimeModule.GetRuntimeAsync();
                ArgumentNullException.ThrowIfNull(runtimerespone?.NodeInfo?.Version);
                return new LoadBalancerPlatformTestingResponseModel() { Success = true, Message = runtimerespone.NodeInfo.Version };
            }
            catch (Exception ex)
            {
                logger.LogDebug($"Error connecting to NSX-T Endpoint {nsxtalbobject.UrlEndpoint} - {ex.Message}");
                return new LoadBalancerPlatformTestingResponseModel() { Success = false, Message = ex.Message };
            }
        }
        public void LoadCredentials()
        {
            nsxtalbobject.Credential = DBContext.Credentials.Single(x => x.Id == nsxtalbobject.CredentialID);
        }
    }
}
