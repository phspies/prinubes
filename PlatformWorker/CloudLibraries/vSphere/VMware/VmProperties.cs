using Prinubes.vCenterSDK;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    public struct VmProperties
    {
        public string Uuid;
        public string Name;
        public string GuestId;
        public string GuestFullName;
        public string HostName;
        public string IPAddress;
        public VirtualMachinePowerState PowerState;
        public ManagedObjectReference Host;
        public bool IsTemplate;
        public int NumCPU;
        public int MemoryMB;
        public string Version;
    }
}
