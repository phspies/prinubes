using Prinubes.vCenterSDK;

namespace PlatformWorker.VMware.Interfaces
{
    public interface IVimCluster : IVimManagedItem
    {
        ClusterProperties ClusterProperties { get; set; }

        void GetCommonProperties(Dictionary<string, object> properties);

        Task GetCommonPropertiesAsync();

        ManagedObjectReference GetVmFolder();
    }
}
