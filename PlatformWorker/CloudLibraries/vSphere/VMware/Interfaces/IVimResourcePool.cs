using Prinubes.vCenterSDK;

namespace PlatformWorker.VMware.Interfaces
{
    public interface IVimResourcePool : IVimManagedItem
    {
        ManagedObjectReference Parent { get; set; }
        void GetCommonProperties(Dictionary<string, object> properties);
    }
}
