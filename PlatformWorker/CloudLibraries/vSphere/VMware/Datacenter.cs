using Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware.Interfaces;
using Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware.VirtualMachine;
using Prinubes.vCenterSDK;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    public class Datacenter : VCManagedItem, IVimDatacenter, IVimManagedItem
    {
        public static string[] VCProperties = new string[3] { "name", "effectiveRole", "vmFolder" };
        private DatacenterProperties _dcProperties;
        private IVimService _vimService;

        public DatacenterProperties DatacenterProperties
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

        public Datacenter(IVimService vimService, ManagedObjectReference managedObject) : base(vimService, managedObject)
        {
            vimService = vimService;
            managedObject = managedObject;
        }

        public void GetCommonProperties()
        {
            if (_dcProperties.Name != null)
                return;
            GetCommonProperties(GetPropertiesAsync(Datacenter.VCProperties).Result);
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
                GetCommonProperties();
            return _dcProperties.Name;
        }

        public ManagedObjectReference GetVmFolder()
        {
            if (_dcProperties.VmFolder == null)
                GetCommonProperties();
            return _dcProperties.VmFolder;
        }

        public IVimVm[] GetVmsAndProperties(ManagedObjectAndProperties[] managedObjectsAndProperties)
        {
            List<IVimVm> vimVmList = new List<IVimVm>();
            foreach (ManagedObjectAndProperties objectsAndProperty in managedObjectsAndProperties)
            {
                if (objectsAndProperty.ManagedObject.type == "VirtualMachine")
                {
                    IVimVm vimVm = new Vm(VcService, objectsAndProperty.ManagedObject);
                    vimVm.GetCommonProperties(objectsAndProperty.Properties);
                    if (!vimVm.VMProperties.IsTemplate)
                        vimVmList.Add(vimVm);
                }
            }
            return vimVmList.ToArray();
        }

        public async Task<IVimVm[]> GetVmsAndPropertiesAsync()
        {
            return GetVmsAndProperties(await GetManagedObjectAndPropertiesAsync(ManagedObject, "vmFolder", "VirtualMachine", Vm.VCProperties));
        }

        public async Task<IVimDatastore[]> GetDatastoresAndPropertiesAsync()
        {
            return GetDatastoresAndProperties(await GetManagedObjectAndPropertiesAsync(ManagedObject, "datastore", "Datastore", Datastore.VCProperties));
        }

        public IVimDatastore[] GetDatastoresAndProperties(ManagedObjectAndProperties[] managedObjectsAndProperties)
        {
            List<Datastore> datastoreList = new List<Datastore>();
            foreach (ManagedObjectAndProperties objectsAndProperty in managedObjectsAndProperties)
            {
                if (!(objectsAndProperty.ManagedObject.type != "Datastore") && objectsAndProperty.Properties != null)
                {
                    Datastore datastore = new Datastore(VcService, objectsAndProperty.ManagedObject);
                    datastore.GetCommonPropertiesAsync(objectsAndProperty.Properties);
                    if (!string.IsNullOrEmpty(datastore.DsProperties.RemoteId))
                        datastoreList.Add(datastore);
                }
            }
            return datastoreList.ToArray();
        }

        public async Task<IVimNetwork[]> GetNetworksAsync()
        {
            ManagedObjectAndProperties[] managedObjectsAndProperties = await GetManagedObjectAndPropertiesAsync(ManagedObject, "network", "Network", Network.VCProperties);
            return await GetNetworksAndPropertiesAsync(managedObjectsAndProperties);
        }

        public async Task<IVimNetwork[]> GetNetworksAndPropertiesAsync(ManagedObjectAndProperties[] managedObjectsAndProperties)
        {
            List<Network> networkList = new List<Network>();
            Dictionary<string, string> virtualPortgroups = await GetDistributedVirtualPortgroupsAsync();
            foreach (ManagedObjectAndProperties objectsAndProperty in managedObjectsAndProperties)
            {
                if ((!(objectsAndProperty.ManagedObject.type != "Network") || !(objectsAndProperty.ManagedObject.type != "DistributedVirtualPortgroup")) && objectsAndProperty.Properties != null)
                {
                    Network network = new Network(VcService, objectsAndProperty.ManagedObject);
                    network.GetCommonPropertiesAsync(objectsAndProperty.Properties);
                    if (!network.IsDistributed || virtualPortgroups.ContainsKey(network.PortgroupKey))
                        networkList.Add(network);
                }
            }
            return networkList.ToArray();
        }

        public async Task<Dictionary<string, string>> GetDistributedVirtualPortgroupsAsync()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            try
            {
                DVSManagerDvsConfigTarget managerDvsConfigTarget = await _vimService.Service.QueryDvsConfigTargetAsync(((VCService)_vimService).DVSManager, ManagedObject, null);
                if (managerDvsConfigTarget.distributedVirtualPortgroup != null)
                {
                    foreach (DistributedVirtualPortgroupInfo virtualPortgroupInfo in managerDvsConfigTarget.distributedVirtualPortgroup)
                    {
                        if (!virtualPortgroupInfo.uplinkPortgroup)
                            Utils.AddOrReplace<string, string>(dictionary, virtualPortgroupInfo.portgroupKey, virtualPortgroupInfo.portgroupName + " (" + virtualPortgroupInfo.switchName + ")");
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return dictionary;
        }

        public async Task<Dictionary<string, string>> GetDistributedVirtualSwitchUuidsAsync()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            try
            {
                DVSManagerDvsConfigTarget managerDvsConfigTarget = await _vimService.Service.QueryDvsConfigTargetAsync(((VCService)_vimService).DVSManager, ManagedObject, null);
                if (managerDvsConfigTarget.distributedVirtualPortgroup != null)
                {
                    foreach (DistributedVirtualPortgroupInfo virtualPortgroupInfo in managerDvsConfigTarget.distributedVirtualPortgroup)
                    {
                        if (!virtualPortgroupInfo.uplinkPortgroup)
                            Utils.AddOrReplace<string, string>(dictionary, virtualPortgroupInfo.portgroupKey, virtualPortgroupInfo.switchUuid);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return dictionary;
        }
    }
}
