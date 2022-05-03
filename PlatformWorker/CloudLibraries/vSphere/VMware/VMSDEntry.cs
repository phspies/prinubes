namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    public class VMSDEntry
    {
        public string Uid;
        public string DisplayName;
        public string Vmsn;
        public string ParentUid;
        public int _LowCreateTime;
        public int _HighCreateTime;
        public string SnapshotN;
        public DateTime CreateTime
        {
            get
            {
                ulong num = (((ulong)(uint)this._HighCreateTime << 32) + (uint)this._LowCreateTime) * 10UL;
                DateTime dateTime = new DateTime(1970, 1, 1);
                dateTime = dateTime.AddTicks((long)num);
                return dateTime;
            }
        }
    }
}
