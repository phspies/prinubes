using Prinubes.vCenterSDK;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    public struct ClusterProperties
    {
        public string Name;
        public int[] EffectiveRoles;
        public ManagedObjectReference VmFolder;
    }
}
