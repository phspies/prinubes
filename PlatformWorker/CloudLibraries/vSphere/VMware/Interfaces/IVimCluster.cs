using Prinubes.vCenterSDK;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware.Interfaces
{
    public interface IVimCluster : IVimManagedItem
    {
        ClusterProperties ClusterProperties { get; set; }

        void GetCommonProperties(Dictionary<string, object> properties);

        Task GetCommonPropertiesAsync();

        ManagedObjectReference GetVmFolder();
    }
}
