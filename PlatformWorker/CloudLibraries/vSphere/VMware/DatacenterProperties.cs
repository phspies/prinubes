using Prinubes.vCenterSDK;

namespace PlatformWorker.VMware
{
    [Serializable]
    public struct DatacenterProperties
    {
        public string Name;
        public int[] EffectiveRoles;
        public ManagedObjectReference VmFolder;
    }
}
