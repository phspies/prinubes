using PlatformWorker.VMware.Interfaces;
using Prinubes.vCenterSDK;

namespace PlatformWorker.VMware
{
    internal class Cluster : VCManagedItem, IVimCluster, IVimManagedItem
    {
        public static string[] VCProperties = new string[3] { "name", "effectiveRole", "vmFolder" };
        private ClusterProperties _dcProperties;

        public ClusterProperties ClusterProperties
        {
            get
            {
                return _dcProperties;
            }
            set
            {
                _dcProperties = value;
            }
        }

        internal Cluster(IVimService vimService, ManagedObjectReference managedObject) : base(vimService, managedObject)
        {

        }

        public async Task GetCommonPropertiesAsync()
        {
            if (_dcProperties.Name != null)
                return;
            GetCommonProperties(await GetPropertiesAsync(Cluster.VCProperties));
        }

        public void GetCommonProperties(Dictionary<string, object> properties)
        {
            _dcProperties.Name = (string)properties["name"];
            _dcProperties.EffectiveRoles = (int[])properties["effectiveRole"];
            _dcProperties.VmFolder = (ManagedObjectReference)properties["vmFolder"];
            Name = _dcProperties.Name;
        }

        public override async Task<IVimManagedItem[]> GetChildrenAsync()
        {
            ManagedObjectAndProperties[] objectAndProperties1 = await GetManagedObjectAndPropertiesAsync(ManagedObject, "hostFolder", "Folder", new string[1] { "name" });
            IVimManagedItem[] vimManagedItemArray = null;
            foreach (ManagedObjectAndProperties objectAndProperties2 in objectAndProperties1)
            {
                IVimFolderInsideDC vimFolderInsideDc = new FolderInsideDC(VcService, objectAndProperties2.ManagedObject);
                vimFolderInsideDc.Name = (string)objectAndProperties2.Properties["name"];
                if (vimFolderInsideDc.Name == "host")
                    vimManagedItemArray = await vimFolderInsideDc.GetChildrenAsync();
            }
            return vimManagedItemArray;
        }

        public override string GetName()
        {
            if (_dcProperties.Name == null)
                GetCommonPropertiesAsync();
            return _dcProperties.Name;
        }

        public ManagedObjectReference GetVmFolder()
        {
            if (_dcProperties.VmFolder == null)
                GetCommonPropertiesAsync();
            return _dcProperties.VmFolder;
        }
    }
}
