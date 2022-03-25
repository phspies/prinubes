using Prinubes.vCenterSDK;

namespace PlatformWorker.VMware
{
    public struct ManagedObjectAndProperties
    {
        public ManagedObjectReference ManagedObject;
        public Dictionary<string, object> Properties;
    }
}
