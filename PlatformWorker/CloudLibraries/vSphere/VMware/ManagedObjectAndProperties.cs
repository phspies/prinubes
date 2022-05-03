using Prinubes.vCenterSDK;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    public struct ManagedObjectAndProperties
    {
        public ManagedObjectReference ManagedObject;
        public Dictionary<string, object> Properties;
    }
}
