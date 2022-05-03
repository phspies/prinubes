namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    [Serializable]
    public class FailoverConfigNic
    {
        public string Name;
        public string SourceNetwork;
        public string TargetNetwork;
        public bool ConnectedAtPowerOn;
    }
}
