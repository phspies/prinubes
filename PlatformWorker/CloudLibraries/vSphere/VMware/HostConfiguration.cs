using Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware.Interfaces;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    [Serializable]
    public struct HostConfiguration
    {
        public short NumCPU;
        public short NumCpuThreads;
        public short NumCpuPkgs;
        public long Memory;
        public IVimNetwork[] Networks;
    }
}
