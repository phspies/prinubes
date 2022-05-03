using Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware.Interfaces;
using Prinubes.vCenterSDK;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    internal class Snapshot : VCManagedItem, IVimSnapshot, IVimManagedItem
    {
        internal Snapshot(IVimService service, ManagedObjectReference managedObject) : base(service, managedObject)
        {
        }

        public override Task<IVimManagedItem[]> GetChildrenAsync()
        {
            return null;
        }
    }
}
