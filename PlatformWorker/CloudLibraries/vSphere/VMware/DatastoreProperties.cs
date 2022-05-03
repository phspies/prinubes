namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    [Serializable]
    public struct DatastoreProperties
    {
        public string Name;
        public string Url;
        public long Capacity;
        public long FreeSpace;
        public long ProvisionedSpace;
        public string Type;
        public string Version;
        public string RemoteId;
        public long MaxFileSize;
    }
}
