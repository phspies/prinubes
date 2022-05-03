using Prinubes.vCenterSDK;
using PlatformWorker.VMware;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware.Interfaces
{
    public interface IVimVm : IVimManagedItem
    {
        string GuestOS { get; }
        VirtualMachinePowerState PowerState { get; }
        string ResourcePoolName { get; }
        string Uuid { get; }
        VmProperties VMProperties { get; }

        Task AddScsiCtrlAsync(int[] busNumbers, ScsiControllerType scsiControllerType, VimClientlContext ctx);
        Task AddVirtualDisksAsync(VmDiskInfo[] diskInfos, VimClientlContext ctx);
        Task AssertSnapshotsAsync(string[] expectedSnapshotNames);
        Task AttachVirtualDisksAsync(IVimVm vm, VimClientlContext ctx);
        void CloneVm(string targetVmName, IVimHost targetEsx, IVimDatastore targetDatastore, VimClientlContext ctx);
        Task CloneVmAsync(string targetVmName, IVimHost targetEsx, IVimDatastore targetDatastore, IVimResourcePool targetResourcePool, string customizationSpecName, string[] ipAddresses, VimClientlContext ctx);
        void CreateSnapshot(string snapName, string description, VimClientlContext ctx);
        void CreateSnapshot(string snapName, string description, VimClientlContext ctx, bool bIncludeMemory, bool bQuiesceFileSystem);
        Task<VmxConfiguration> CreateSourceVmdkMappingsAsync(VmxConfiguration vmxCgf, IVimDatastore[] datastores, IVimHost host, GetBaseVmdksDelegate getBaseVmdks);
        Task CreateVirtualDisksAsync(VmDiskInfo[] diskInfos, VimClientlContext ctx);
        Task DetachVirtualDisksAsync(IVimVm vm, VimClientlContext ctx);
        Task EditNetworkAsync(VirtualEthernetCard[] ethernetCards, Network network, VimClientlContext ctx);
        Task<VmdkProperties[]> GetActiveDiskFilesLayoutAsync();
        Task<VmProperties> GetCommonPropertiesAsync();
        void GetCommonProperties(Dictionary<string, object> vmProperties);
        VmProperties GetCommonProperties(ManagedObjectAndProperties[] managedObjects);
        Task<VirtualMachineConfigSpec> GetCompatibleConfigSpecAsync(string targetDatastore, string replicaDisplayName);
        Task<VmConfiguration> GetConfigurationAsync();
        Task<IVimDatastore[]> GetDatastoresAndPropertiesAsync();
        IVimDatastore[] GetDatastoresAndProperties(ManagedObjectAndProperties[] data);
        Task<Dictionary<string, VirtualEthernetCard>> GetEthernetCardsAsync();
        Task<VirtualEthernetCard[]> GetEthernetCardsAsync(IVimNetwork network);
        IVimHost GetHost(ManagedObjectAndProperties[] data);
        Task<IVimHost> GetHostAndPropertiesAsync();
        Task<IVimHost> GetHostWithoutPropertiesAsync();
        Task<ManagedObjectAndProperties[]> GetManagedObjectsAndPropertiesAsync();
        string GetName();
        Task<IVimNetwork[]> GetNetworksAndPropertiesAsync();
        IVimNetwork[] GetNetworksAndProperties(ManagedObjectAndProperties[] managedObjectsAndProperties);
        Task<int> GetNumOfPCIDevicesAsync();
        IVimDatastore GetPrimaryDatastore();
        IVimDatastore GetPrimaryDatastore(IVimDatastore[] datastores, string rawVmxPath);
        Task<string> GetRawVmxPathAsync();
        Task<IVimTask[]> GetRecentTasksAsync();
        Task<IVimResourcePool> GetResourcePoolAsync();
        Task<ScsiControllerInfo[]> GetScsiControllersInfoAsync();
        Task<IVimSnapshot> GetSnapshotAsync(string snapName);
        Task<SnapshotsSummary> GetSnapshotsSummaryAsync(string vmsdFile);
        Task<Dictionary<string, string>> GetVirtualDiskLabelsAsync();
        Task<Dictionary<string, long>> GetVirtualDiskSizeAsync();
        Task<VmdkProperties[]> GetVMDKInfoAsync();
        Task<VmdkProperties[]> GetVMDKInfoAsync(IVimDatastore[] datastores);
        Task<VirtualMachineToolsStatus> GetVMToolsStatusAsync();
        Task<string> GetVmxFullNameAsync();
        string GetVmxFullName(IVimDatastore datastore, string rawVmxPath, string datastorePath);
        string GetVmxPath(IVimDatastore datastore, string rawVmxPath, string datastorePath);
        bool HasSnapshotableDisks(VmdkProperties[] vmdkInfos);
        Task<bool> HasUserDefinedSnapshotsAsync();
        Task<bool> IsNicChangedAsync(FailoverConfig failoverConfig);
        Task<bool> IsPoweredOnAsync();
        Task<bool> IsSuspendedAsync();
        bool IsVmdkChanged(ref VmxConfiguration srcCfg, IVimDatastore[] datastores, IVimHost host, GetBaseVmdksDelegate getBaseVmdks);
        Task MigrateVmAsync(IVimHost targetHost);
        Task MountVMToolsAsync();
        Task PowerOffAsync(VimClientlContext ctx);
        Task PowerOnAsync(VimClientlContext ctx);
        Task RebootGuestOSAsync(VimClientlContext ctx);
        Task ReconfigureAsync(string op, VirtualMachineConfigSpec spec, VimClientlContext ctx);
        void RemoveAllSnapshots(VimClientlContext ctx);
        Task RemoveAllVirtualDisksAsync(RemoveDisk removeDisk, VimClientlContext ctx);
        void RemoveSnapshot(ManagedObjectReference mor, VimClientlContext ctx);
        Task RemoveSnapshotAsync(string snapName, VimClientlContext ctx);
        Task RemoveVirtualDisksAsync(string[] names, VimClientlContext ctx);
        Task RemoveVirtualDisksAsync(VmdkProperties[] VmdkProps, VimClientlContext ctx);
        Task RemoveVirtualDisksByFilenameAsync(string[] filenames, VimClientlContext ctx);
        string ReplaceDatastoreName(IVimDatastore[] datastores, string file);
        Task RevertToLastSnapshotAsync(VimClientlContext ctx);
        Task RevertToSnapshotAsync(string snapName, VimClientlContext ctx);
        void SetConfiguration(FailoverConfig failoverConfig, VimClientlContext ctx);
        Task SetConfigurationAsync(FailoverConfig failoverConfig, bool disableAllNics, VimClientlContext ctx);
        Task SetNicConnectivityAsync(bool enableConnectivity, VimClientlContext ctx);
        Task SetNicVirtualSwitchesAsync(string[] virtualSwitchNames, VimClientlContext ctx);
        Task ShutdownGuestOSAsync(VimClientlContext ctx);
        Task TryRemoveAllSnapshotsAsync();
        Task UnmountVMToolsAsync();
        Task UpgradeVMToolsAsync(VimClientlContext ctx);
    }
}
