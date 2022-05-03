namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    [Serializable]
    public struct NetworkProperties
    {
        public string Name;
        public bool IsDistributed;
        public string PortgroupKey;
    }
}
