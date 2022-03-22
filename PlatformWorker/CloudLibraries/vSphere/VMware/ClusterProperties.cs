using Prinubes.vCenterSDK;

namespace PlatformWorker.VMware
{
    public struct ClusterProperties
    {
        public string Name;
        public int[] EffectiveRoles;
        public ManagedObjectReference VmFolder;
    }
}
