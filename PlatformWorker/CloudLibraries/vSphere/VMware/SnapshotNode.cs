using System;
using System.Collections.Generic;
using Prinubes.vCenterSDK;

namespace PlatformWorker.VMware
{
    public class SnapshotNode
    {
        public string DisplayName;
        public string Description;
        public DateTime CreateTime;
        public string Vmsn;
        public ManagedObjectReference Mor;
        public List<string> Vmdks;
        public List<SnapshotNode> Children;

        public SnapshotNode()
        {
            this.Children = new List<SnapshotNode>();
            this.Vmdks = new List<string>();
        }
    }
}
