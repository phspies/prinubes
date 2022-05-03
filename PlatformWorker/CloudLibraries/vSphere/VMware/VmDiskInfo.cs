using Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware.Interfaces;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    public class VmDiskInfo
    {
        public bool Exists;
        public string File;
        public int CtrlKey;
        public int UnitNumber;
        public long SizeMB;
        public string Mode;
        public IVimDatastore Location;
        public string DiskType;
        public string PreExistingDiskPath;

        public VmDiskInfo(bool exists, string file, int ctrlKey, int unitNumber, long sizeMB, string mode, IVimDatastore location, string preexistingDiskPath)
        {
            this.Exists = exists;
            this.File = file;
            this.CtrlKey = ctrlKey;
            this.UnitNumber = unitNumber;
            this.SizeMB = sizeMB;
            this.Mode = mode;
            this.Location = location;
            this.PreExistingDiskPath = preexistingDiskPath;
        }

        public VmDiskInfo(bool exists, string file, int ctrlKey, int unitNumber, long sizeMB, string mode, IVimDatastore location, string diskType, string preExistingDiskPath)
        {
            this.Exists = exists;
            this.File = file;
            this.CtrlKey = ctrlKey;
            this.UnitNumber = unitNumber;
            this.SizeMB = sizeMB;
            this.Mode = mode;
            this.Location = location;
            this.DiskType = diskType;
            this.PreExistingDiskPath = preExistingDiskPath;
        }
    }
}
