using PlatformWorker.VMware.Interfaces;
using System;

namespace PlatformWorker.VMware
{
    public class VmdkProperties
    {
        public Guid Guid = Guid.Empty;
        public string FileName;
        public string Label;
        public long Capacity;
        public string DatastoreName;
        public string DatastoreUrl;
        public string DiskMode;
        public int Key;
        public int UnitNumber;
        public int ControllerKey;
        public int BusNumber;
        public IVimDatastore Datastore;
        public bool ThinProvisioned;

        public VmdkProperties Clone()
        {
            VmdkProperties vmdkProperties = new VmdkProperties();
            vmdkProperties.FileName = this.FileName;
            vmdkProperties.Label = this.Label;
            vmdkProperties.Capacity = this.Capacity;
            vmdkProperties.DatastoreName = this.DatastoreName;
            vmdkProperties.DatastoreUrl = this.DatastoreUrl;
            vmdkProperties.DiskMode = this.DiskMode;
            vmdkProperties.Key = this.Key;
            vmdkProperties.UnitNumber = this.UnitNumber;
            vmdkProperties.ControllerKey = this.ControllerKey;
            vmdkProperties.BusNumber = this.BusNumber;
            vmdkProperties.Datastore = this.Datastore;
            int num = this.ThinProvisioned ? 1 : 0;
            vmdkProperties.ThinProvisioned = num != 0;
            Guid guid = this.Guid;
            vmdkProperties.Guid = guid;
            return vmdkProperties;
        }
    }
}
