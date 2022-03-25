using PlatformWorker.VMware.Interfaces;
using Prinubes.vCenterSDK;
using ObjectContent = Prinubes.vCenterSDK.ObjectContent;

namespace PlatformWorker.VMware
{
    internal abstract class BrowseVimInventoryBase
    {
        protected Dictionary<string, InventoryNode> _vcInventoryNodes = new Dictionary<string, InventoryNode>(StringComparer.CurrentCultureIgnoreCase);
        protected IVimService _vimService;

        public BrowseVimInventoryBase(IVimService vimService)
        {
            this._vimService = vimService;
        }

        protected void SetInventoryNodeChilds()
        {
            foreach (InventoryNode inventoryNode in this._vcInventoryNodes.Values)
            {
                if (inventoryNode.Parent != null)
                {
                    ManagedObjectReference parent = inventoryNode.Parent;
                    if (this._vcInventoryNodes.ContainsKey(parent.Value))
                        this._vcInventoryNodes[parent.Value].Children.Add(inventoryNode.ManagedObject);
                }
            }
        }

        protected virtual void RemoveNotRequiredManagedObjects()
        {
        }

        public virtual Task<Dictionary<string, InventoryNode>> LoadInventoryAsync()
        {
            return null;
        }

        protected void BuildInventoryNodesDictionary(ObjectContent[] contents)
        {
            string name = null;
            ManagedObjectReference parent = null;
            foreach (ObjectContent content in contents)
            {
                ManagedObjectReference managedObject = content.obj;
                foreach (DynamicProperty prop in content.propSet)
                {
                    if (prop.name == "name")
                        name = (string)prop.val;
                    else if (prop.name == "parent")
                        parent = (ManagedObjectReference)prop.val;
                }
                InventoryNode inventoryNode = new InventoryNode(name, managedObject, parent);
                if (!this._vcInventoryNodes.ContainsKey(managedObject.Value))
                    this._vcInventoryNodes.Add(managedObject.Value, inventoryNode);
                parent = null;
            }
        }
    }
}
