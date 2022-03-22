using System;

namespace PlatformWorker.VMware
{
    public class VmdkMapping
    {
        public string DatastoreName;
        public string DatastoreUrl;
        public string VmdkDir;
        public string VmdkFullName;
        public string VmdkFullNameBin;
        public string VmdkShortName;
        public string VmdkBaseName;
        public string VmdkShortNameBin;
        public long VmdkSize;
        public bool IsSnapshotable;
        public long TimeStamp;
        public int Key;
        public string Label;
    }
}
