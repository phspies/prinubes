using Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware.Interfaces;
using Prinubes.vCenterSDK;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    internal class FolderInsideDC : VCManagedItem, IVimFolderInsideDC, IVimManagedItem
    {
        internal FolderInsideDC(IVimService vimService, ManagedObjectReference managedObject) : base(vimService, managedObject)
        {
        }

        public override async Task<IVimManagedItem[]> GetChildrenAsync()
        {
            List<IVimManagedItem> vimManagedItemList = new List<IVimManagedItem>();
            string[] properties1 = new string[1] { "childEntity" };
            foreach (ManagedObjectReference managedObject in await GetManagedObjectsAsync(properties1))
            {
                if (managedObject.type == "ComputeResource" || managedObject.type == "ClusterComputeResource")
                {
                    foreach (ManagedObjectAndProperties objectAndProperty in await (new FolderInsideDC(VcService, managedObject)).GetManagedObjectAndPropertiesAsync(managedObject, "host", "HostSystem", Host.VCProperties))
                    {
                        IVimManagedItem vimManagedItem = new Host(VcService, objectAndProperty.ManagedObject);
                        await ((IVimHost)vimManagedItem).GetCommonPropertiesAsync(objectAndProperty.Properties);
                        vimManagedItemList.Add(vimManagedItem);
                    }
                }
                else
                {
                    IVimManagedItem vimManagedItem = new FolderInsideDC(VcService, managedObject);
                    Dictionary<string, object> properties2 = await vimManagedItem.GetPropertiesAsync(new string[1] { "name" });
                    vimManagedItem.Name = (string)properties2["name"];
                    vimManagedItemList.Add(vimManagedItem);
                }
            }
            return vimManagedItemList.ToArray();
        }

        public override string GetName()
        {
            Name = (string)GetPropertiesAsync(new string[1] { "name" }).Result["name"];
            return Name;
        }
    }
}
