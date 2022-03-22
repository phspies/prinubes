using PlatformWorker.VMware.Interfaces;
using System;

namespace PlatformWorker.VMware
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
