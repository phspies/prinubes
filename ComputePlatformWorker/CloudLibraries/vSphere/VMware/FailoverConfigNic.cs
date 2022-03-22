using System;

namespace PlatformWorker.VMware
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
