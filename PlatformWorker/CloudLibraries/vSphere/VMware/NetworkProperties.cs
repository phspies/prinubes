namespace PlatformWorker.VMware
{
    [Serializable]
    public struct NetworkProperties
    {
        public string Name;
        public bool IsDistributed;
        public string PortgroupKey;
    }
}
