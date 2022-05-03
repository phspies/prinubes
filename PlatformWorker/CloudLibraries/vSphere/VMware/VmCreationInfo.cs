using Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware.Interfaces;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    public class VmCreationInfo
    {
        public string Name;
        public string GuestId;
        public int NumCPU;
        public int NumCoresPerProcessor;
        public long MemoryMB;
        public ScsiControllerType ScsiControllerType;
        public VirtualNicType NicType;
        public string[] NICMapping;
        public VmDiskInfo[] Disks;
        public IVimDatastore Location;

        public VmCreationInfo(string name, string guestId, int numCPU, int numCoresPerProcessor, long memoryMB, ScsiControllerType scsiControllerType, VirtualNicType nicType, string[] nicMapping, VmDiskInfo[] disks, IVimDatastore location)
        {
            this.Name = name;
            this.GuestId = guestId;
            this.NumCPU = numCPU;
            this.NumCoresPerProcessor = numCoresPerProcessor;
            this.MemoryMB = memoryMB;
            this.ScsiControllerType = scsiControllerType;
            this.NicType = nicType;
            this.NICMapping = nicMapping;
            this.Disks = disks;
            this.Location = location;
        }
    }
}
