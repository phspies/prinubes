using PlatformWorker.VMware.Interfaces;
using Prinubes.vCenterSDK;

namespace PlatformWorker.VMware
{
    internal class BrowseForVmInventory : BrowseVimInventoryBase
    {
        internal BrowseForVmInventory(IVimService vimService) : base(vimService)
        {
        }

        protected override void RemoveNotRequiredManagedObjects()
        {
            foreach (InventoryNode inventoryNode in this._vcInventoryNodes.Values)
            {
                if (inventoryNode.Name == "vm")
                {
                    InventoryNode vcInventoryNode1 = this._vcInventoryNodes[inventoryNode.Parent.Value];
                    vcInventoryNode1.Children.Remove(inventoryNode.ManagedObject);
                    foreach (ManagedObjectReference child in inventoryNode.Children)
                    {
                        if (this._vcInventoryNodes.ContainsKey(child.Value))
                        {
                            InventoryNode vcInventoryNode2 = this._vcInventoryNodes[child.Value];
                            vcInventoryNode2.Parent = inventoryNode.Parent;
                            vcInventoryNode1.Children.Add(vcInventoryNode2.ManagedObject);
                        }
                    }
                }
            }
            Dictionary<string, InventoryNode> dictionary = new Dictionary<string, InventoryNode>(StringComparer.CurrentCultureIgnoreCase);
            foreach (InventoryNode inventoryNode in this._vcInventoryNodes.Values)
            {
                if (!(inventoryNode.Name == "vm"))
                    dictionary.Add(inventoryNode.ManagedObject.Value, inventoryNode);
            }
            this._vcInventoryNodes.Clear();
            this._vcInventoryNodes = dictionary;
        }

        public override async Task<Dictionary<string, InventoryNode>> LoadInventoryAsync()
        {
            TraversalSpec traversalSpec1 = new TraversalSpec();
            traversalSpec1.name = "datacenterHostTraversalSpec";
            traversalSpec1.type = "Datacenter";
            traversalSpec1.path = "vmFolder";
            traversalSpec1.skip = false;
            traversalSpec1.selectSet = new SelectionSpec[1]
            {
        new SelectionSpec()
            };
            traversalSpec1.selectSet[0].name = "folderTraversalSpec";
            TraversalSpec traversalSpec2 = new TraversalSpec();
            traversalSpec2.name = "folderTraversalSpec";
            traversalSpec2.type = "Folder";
            traversalSpec2.path = "childEntity";
            traversalSpec2.skip = false;
            traversalSpec2.selectSet = new SelectionSpec[2]
            {
        new SelectionSpec(),
         traversalSpec1
            };
            traversalSpec2.selectSet[0].name = "folderTraversalSpec";
            PropertySpec[] propertySpecArray = new PropertySpec[1] { new PropertySpec() };
            propertySpecArray[0].all = false;
            propertySpecArray[0].pathSet = new string[2]
            {
        "name",
        "parent"
            };
            propertySpecArray[0].type = "ManagedEntity";
            PropertyFilterSpec propertyFilterSpec = new PropertyFilterSpec();
            propertyFilterSpec.propSet = propertySpecArray;
            propertyFilterSpec.objectSet = new ObjectSpec[1]
            {
        new ObjectSpec()
            };
            propertyFilterSpec.objectSet[0].obj = this._vimService.RootFolder.ManagedObject;
            propertyFilterSpec.objectSet[0].skip = false;
            propertyFilterSpec.objectSet[0].selectSet = new SelectionSpec[] { traversalSpec2 };
            this.BuildInventoryNodesDictionary(await _vimService.RetrievePropertiesAsync(new PropertyFilterSpec[] { propertyFilterSpec }));
            this.SetInventoryNodeChilds();
            this.RemoveNotRequiredManagedObjects();
            return this._vcInventoryNodes;
        }
    }
}
