using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Models;

namespace UnitTesting
{
    static class GlobalVariables
    {
        public static PlatformApplicationFactory? platformFactory;
        public static IdentityApplicationFactory? identityFactory;
        public static PlatformWorkerApplicationFactory? platformWorkerFactory;


        public static CredentialDisplayDataModel? SessionvCenterCredentials;
        public static CredentialDisplayDataModel? SessionNSXTCredentials;
        public static CredentialDisplayDataModel? SessionNSXALBCredentials;

        public static OrganizationDisplayDataModel? SessionOrganization;

        public static AuthenticateResponse? SessionToken;
        public static GroupDisplayDataModel? SessionGroup;

        public static NetworkPlatformDisplayDataModel? SessionNetworkObject;
        public static LoadBalancerPlatformDisplayDataModel? SessionLoadBalancerObject;
        public static ComputePlatformDisplayDataModel? SessionComputeObject;
    }
}
