using System;

namespace PlatformWorker.VMware
{
    [Serializable]
    public struct ServerProperties
    {
        public string Name;
        public string Uuid;
        public string Version;
        public string BuildNum;
        public string Vendor;
        public string Model;
        public string ProcessorType;
        public short NumOfCpu;
        public short NumOfCpuPkgs;
        public short NumOfCpuThreads;
        public int NumOfNics;
        public int CpuMHz;
        public long MemoryMB;
        public int CpuUsageMHz;
        public long MemoryUsageMB;
    }
}
