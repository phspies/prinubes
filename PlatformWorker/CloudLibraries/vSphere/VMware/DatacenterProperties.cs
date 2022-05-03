using Prinubes.vCenterSDK;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    [Serializable]
    public struct DatacenterProperties
    {
        public string Name;
        public int[] EffectiveRoles;
        public ManagedObjectReference VmFolder;
    }
}
