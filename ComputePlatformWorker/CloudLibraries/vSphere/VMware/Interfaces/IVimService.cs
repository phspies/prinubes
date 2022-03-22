using Prinubes.vCenterSDK;
using System;
using System.Collections.Generic;
using Prinubes.vCenterSDK;
using System.Threading.Tasks;

namespace PlatformWorker.VMware.Interfaces
{
    public interface IVimService
    {
        string ApiVersion { get; }
        Task<DateTime> CurrentTime { get; }
        ManagedObjectReference FileManager { get; }
        string FullName { get; }
        bool IsVirtualCenter { get; }
        ManagedObjectReference PropertyCollector { get; }
        IVimFolderOutsideDC RootFolder { get; }
        VimPortType Service { get; }
        ManagedObjectReference VirtualDiskManager { get; }

        Task<List<IVimDatastore>> GetAllDatastoresAsync();
        Task<Dictionary<string, IVimHost>> GetAllHostsDictAsync();
        Task<ObjectContent[]> GetAllHostsObjectContentsAsync();
        Task<Dictionary<string, IVimVm>> GetAllVMsDictWithNameAsync();
        Task<Dictionary<string, IVimVm>> GetAllVMsDictWithNameAsync(ManagedObjectReference _datacenter);
        Task<Dictionary<string, IVimVm>> GetAllVMsDictWithUuidAsync();
        Task<CustomizationSpecItem> GetCustomizationSpecAsync(string name);
        IVimDatacenter GetDatacenter(ManagedObjectReference managedObject);
        IVimDatacenter[] GetDatacenters();
        Task<IVimDatastore> GetDatastoreByNameAsync(string name);
        Task<IVimDatastore> GetDatastoreByUrlAsync(string url);
        IVimFolderInsideDC GetFolderInsideDC(ManagedObjectReference managedObject);
        IVimFolderOutsideDC GetFolderOutsideDC(ManagedObjectReference managedObject);
        Task<IVimHost> GetHostAsync(string name);
        Task<IVimHost> GetHostAsync(string name, bool retrieveCommonProperties);
        Task<Dictionary<string, InventoryNode>> GetHostInventoryAsync();
        IVimHost GetHostManagedItem(ManagedObjectReference managedObject);
        Task<IVimHost[]> GetHostsAsync(IVimDatastore[] datastores);
        Task<IVimHost> GetHostWithoutCaseInsensetiveComparison(string name);
        Task<ObjectContent[]> getObjectContentsAsync(ManagedObjectReference[] managedObjects, string[] properties);
        Task<Dictionary<ManagedObjectReference, Dictionary<string, object>>?> GetPropertiesAsync(IVimManagedItem[] items, string[] properties);
        Task<Dictionary<ManagedObjectReference, Dictionary<string, object>>> GetPropertiesAsync(ManagedObjectReference[] managedObjects, string[] properties);
        InventoryNode GetRootFolderOfInventory(Dictionary<string, InventoryNode> inventory);
        IVimVm GetVm(ManagedObjectReference managedObject);
        Task<Dictionary<string, InventoryNode>> GetVmInventoryAsync();
        Task<IVimVm> GetVmOrVmTemplateAsync(string name);
        Task HeartbeatAsync();
        void LogOff();
        void Logon();
        Dictionary<string, object> PropSetToDictionary(DynamicProperty[] dynamicProperties);
        Task<ObjectContent[]> RetrievePropertiesAsync(PropertyFilterSpec[] pfSpec);
        Task<List<string>> SearchDatastoreSubFolderAsync(string esxHost, string folderName, VimClientlContext ctx);
        Task<IVimHost> SearchHostByDnsNameAsync(string dnsName, bool retrieveCommonProperties);
        Task<IVimHost> SearchHostByIPAsync(string ip, bool retrieveCommonProperties);
        Task<IVimHost> SearchHostByUuidAsync(string uuid, bool retrieveCommonProperties);
        Task<IVimVm> SearchVmByDnsNameAsync(string dnsName);
        Task<IVimVm> SearchVmByDnsNameAsync(string dnsName, bool retrieveCommonProperties);
        Task<IVimVm> SearchVmByUuidAsync(string uuid);
        Task<IVimVm> SearchVmByUuidAsync(string uuid, bool retrieveCommonProperties);
        void Shutdown();
        void UnregisterAndDestroyVm(IVimVm vm, VimClientlContext ctx);
        Task UnregisterVmAsync(IVimVm vm);
        ManagedObjectReference[]? VCManagedItemsToMors(IVimManagedItem[] items);
    }
}
