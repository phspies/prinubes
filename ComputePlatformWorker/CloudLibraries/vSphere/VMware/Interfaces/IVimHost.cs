using System.Collections.Generic;
using System.Threading.Tasks;
using Prinubes.vCenterSDK;

namespace PlatformWorker.VMware.Interfaces
{
  public interface IVimHost : IVimManagedItem
  {
        ServerProperties Properties { get; set; }
        Task<string> ContainsVmNameAsync(string vmName);
        Task<IVimVm> CreateVmAsync(VirtualMachineConfigSpec configSpec, VimClientlContext ctx);
        Task<IVimVm> CreateVmAsync(VmCreationInfo vmCreationInfo, VimClientlContext ctx);
        Task<IVimVm> CreateVmAsync(VmCreationInfo vmCreationInfo, VimClientlContext ctx, string template, ManagedObjectReference chosenDatastore, ResourcePool resourcePool, string templateUUID);
        Task<IVimVm> CreateVmWithNetworkMappingAsync(VirtualMachineConfigSpec configSpec, Dictionary<string, string> networkMap, VimClientlContext ctx);
        Task<IVimResourcePool[]> GetAllResourcePoolsAsync();
        Task<IVimManagedItem[]> GetChildrenAsync();
        ServerProperties GetCommonProperties();
        Task GetCommonPropertiesAsync(Dictionary<string, object> hostProperties);
        Task<ServerProperties> GetCommonPropertiesAsync(ManagedObjectAndProperties[] managedObjects);
        Task<ManagedObjectReference> GetComputeResourceAsync();
        Task<HostConfiguration> GetConfigurationAsync();
        Task<IVimDatacenter> GetDatacenterAndPropertiesAsync();
        Task<IVimDatastore> GetDatastoreByNameAsync(string name);
        Task<IVimDatastore> GetDatastoreByUrlAsync(string url);
        long GetDatastoreMaxVmdkSizeMB(IVimDatastore ds);
        Task<long> GetDatastoreMaxVmdkSizeMBAsync(string url);
        Task<string> GetDatastorePathByUrlAsync(string url);
        Task<IVimDatastore[]> GetDatastoresAndPropertiesAsync();
        IVimDatastore[] GetDatastoresAndProperties(ManagedObjectAndProperties[] managedObjectsAndProperties);
        Task<IVimResourcePool> GetDefaultResourcePoolAsync();
        Task<Dictionary<string, string>> GetDistributedVirtualPortgroupsAsync();
        Task<Dictionary<string, string>> GetDistributedVirtualSwitchUuidsAsync();
        Task<IVimHost[]> GetHostsAsync();
        Task<ManagedObjectAndProperties[]> GetMangedObjectsAndPropertiesAsync();
        Task<long> GetMemoryAsync();
        string GetName();
        Task<IVimNetwork[]> GetNetworksAndPropertiesAsync(ManagedObjectAndProperties[] managedObjectsAndProperties);
        Task<IVimNetwork[]> GetNetworksAsync();
        Task<short> GetNumberCPUAsync();
        Task<short> GetNumberCpuPackagesAsync();
        Task<short> GetNumberCpuThreadsAsync();
        Task<IVimVm> GetRecentlyCreatedVmAsync(ManagedObjectReference task);
        Task<IVimResourcePool> GetResourcePoolByNameAsync(string resPoolName);
        Task<string> GetUuidAsync();
        Task<Dictionary<string, string[]>> GetVirtualSwitchAsync();
        Task<IVimVm> GetVmAsync(string name);
        Task<IVimVm> GetVmByUuidAsync(string uuid);
        Task<IVimVm[]> GetVmsAndPropertiesAsync();
        IVimVm[] GetVmsAndProperties(ManagedObjectAndProperties[] managedObjectsAndProperties);
        Task<IVimVm[]> GetVmTemplatesAndPropertiesAsync();
        Task MoveVmToResourcePoolAsync(IVimVm vm, string resPoolName);
        Task<IVimVm> RegisterVmAsync(string dsPath, string resPoolName, VimClientlContext ctx);
        Task<List<string>> SearchDatastoreSubFolderAsync(string folderName, VimClientlContext ctx);
        Task<IVimVm> SearchVmByUuidAsync(string uuid);
        Task UnregisterVmAsync(IVimVm vm);
        bool UserHasPermissions(out int userRole);
    }
}
