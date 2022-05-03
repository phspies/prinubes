using Prinubes.vCenterSDK;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware.Interfaces
{
    public interface IVimResourcePool : IVimManagedItem
    {
        ManagedObjectReference Parent { get; set; }
        void GetCommonProperties(Dictionary<string, object> properties);
    }
}
