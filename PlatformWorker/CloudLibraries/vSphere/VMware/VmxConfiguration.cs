using Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware.Interfaces;
using System.Runtime.Serialization;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    [Serializable]
    public class VmxConfiguration
    {
        public string HostName;
        [OptionalField(VersionAdded = 2)]
        public string Username;
        public string HostAlias;
        public string VmGuid;
        public string PrimaryDatastoreUrl;
        public string PrimaryDatastorePath;
        public string VmDisplayName;
        public string VmxPath;
        public string VmxShortName;
        public VmdkMapping[] VmdkMappings;
        public string PassthruScsi;
        public string ResourcePool;

        public bool IsMultiDatastores
        {
            get
            {
                List<string> stringList = new List<string>();
                foreach (VmdkMapping vmdkMapping in this.VmdkMappings)
                {
                    if (!stringList.Contains(vmdkMapping.DatastoreUrl))
                    {
                        if (stringList.Count > 0)
                            return true;
                        stringList.Add(vmdkMapping.DatastoreUrl);
                    }
                }
                return false;
            }
        }

        public object ShallowCopy()
        {
            return this.MemberwiseClone();
        }

        public long GetTotalSnapshotableVmdksSize()
        {
            long num = 0;
            if (this.VmdkMappings != null)
            {
                foreach (VmdkMapping vmdkMapping in this.VmdkMappings)
                {
                    if (vmdkMapping.IsSnapshotable)
                        num += vmdkMapping.VmdkSize;
                }
            }
            return num;
        }

        public static VmdkMapping[] CreateTargetVmdkMappings(VmdkMapping[] srcVmdkMappings, IVimDatastore ds, string vmxPath)
        {
            List<VmdkMapping> vmdkMappingList = new List<VmdkMapping>();
            List<string> vmdkBaseNames = new List<string>();
            foreach (VmdkMapping srcVmdkMapping in srcVmdkMappings)
            {
                VmdkMapping vmdkMapping = new VmdkMapping();
                vmdkMapping.IsSnapshotable = srcVmdkMapping.IsSnapshotable;
                vmdkMapping.Key = srcVmdkMapping.Key;
                vmdkMapping.Label = srcVmdkMapping.Label;
                vmdkMapping.VmdkSize = 0L;
                vmdkMapping.DatastoreName = ds.DsProperties.Name;
                vmdkMapping.DatastoreUrl = ds.DsProperties.Url;
                string str1 = VmxConfiguration.ResolveBaseName(srcVmdkMapping.VmdkBaseName, vmdkBaseNames);
                string extension = Path.GetExtension(srcVmdkMapping.VmdkShortName);
                string str2 = str1 + extension;
                string str3 = str1 + "-flat" + extension;
                vmdkMapping.VmdkDir = vmxPath;
                vmdkMapping.VmdkBaseName = str1;
                vmdkMapping.VmdkShortName = str2;
                vmdkMapping.VmdkShortNameBin = str3;
                vmdkMapping.VmdkFullName = vmxPath + "/" + str2;
                vmdkMapping.VmdkFullNameBin = vmxPath + "/" + str3;
                vmdkMappingList.Add(vmdkMapping);
            }
            return vmdkMappingList.ToArray();
        }

        private static string ResolveBaseName(string name, List<string> vmdkBaseNames)
        {
            int num = 1;
            string str = name;
            while (vmdkBaseNames.Contains(str))
            {
                str = name + "_" + num;
                ++num;
            }
            vmdkBaseNames.Add(str);
            return str;
        }
    }
}
