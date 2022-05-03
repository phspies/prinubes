using Prinubes.vCenterSDK;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware.Interfaces
{
    public interface IVimDatacenter : IVimManagedItem
    {
        DatacenterProperties DatacenterProperties { get; set; }

        void GetCommonProperties(Dictionary<string, object> properties);

        void GetCommonProperties();

        ManagedObjectReference GetVmFolder();
    }
}
