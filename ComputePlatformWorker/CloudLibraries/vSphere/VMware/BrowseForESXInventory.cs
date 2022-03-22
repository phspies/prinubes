using PlatformWorker.VMware.Interfaces;
using System;
using System.Collections.Generic;
using Prinubes.vCenterSDK;
using System.Threading.Tasks;

namespace PlatformWorker.VMware
{
    internal class BrowseForESXInventory : BrowseVimInventoryBase
    {
        internal BrowseForESXInventory(IVimService vimService) : base(vimService)
        {
        }

        protected override void RemoveNotRequiredManagedObjects()
        {
            foreach (InventoryNode inventoryNode in this._vcInventoryNodes.Values)
            {
                if (inventoryNode.Name == "host" || inventoryNode.ManagedObject.type == "ComputeResource" || inventoryNode.ManagedObject.type == "ClusterComputeResource")
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
            Dictionary<string, InventoryNode> dictionary = new Dictionary<string, InventoryNode>((IEqualityComparer<string>)StringComparer.CurrentCultureIgnoreCase);
            foreach (InventoryNode inventoryNode in this._vcInventoryNodes.Values)
            {
                if (!(inventoryNode.Name == "host") && !(inventoryNode.ManagedObject.type == "ComputeResource") && !(inventoryNode.ManagedObject.type == "ClusterComputeResource"))
                    dictionary.Add(inventoryNode.ManagedObject.Value, inventoryNode);
            }
            this._vcInventoryNodes.Clear();
            this._vcInventoryNodes = dictionary;
        }

        public override async Task<Dictionary<string, InventoryNode>> LoadInventoryAsync()
        {
            TraversalSpec traversalSpec1 = new TraversalSpec();
            traversalSpec1.name = "computeResourceHostTraversalSpec";
            traversalSpec1.type = "ComputeResource";
            traversalSpec1.path = "host";
            traversalSpec1.skip = false;
            TraversalSpec traversalSpec2 = new TraversalSpec();
            traversalSpec2.name = "datacenterHostTraversalSpec";
            traversalSpec2.type = "Datacenter";
            traversalSpec2.path = "hostFolder";
            traversalSpec2.skip = false;
            traversalSpec2.selectSet = new SelectionSpec[1]
            {
        new SelectionSpec()
            };
            traversalSpec2.selectSet[0].name = "folderTraversalSpec";
            TraversalSpec traversalSpec3 = new TraversalSpec();
            traversalSpec3.name = "folderTraversalSpec";
            traversalSpec3.type = "Folder";
            traversalSpec3.path = "childEntity";
            traversalSpec3.skip = false;
            traversalSpec3.selectSet = new SelectionSpec[3]
            {
        new SelectionSpec(),
        (SelectionSpec) traversalSpec2,
        (SelectionSpec) traversalSpec1
            };
            traversalSpec3.selectSet[0].name = "folderTraversalSpec";
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
            propertyFilterSpec.objectSet[0].obj = _vimService.RootFolder.ManagedObject;
            propertyFilterSpec.objectSet[0].skip = false;
            propertyFilterSpec.objectSet[0].selectSet = new SelectionSpec[1]
            {
        (SelectionSpec) traversalSpec3
            };
            ObjectContent[] contents = await _vimService.RetrievePropertiesAsync(new PropertyFilterSpec[1] { propertyFilterSpec });  
            this.BuildInventoryNodesDictionary(contents);
            this.BuildInventoryNodesDictionary(contents);
            this.SetInventoryNodeChilds();
            this.RemoveNotRequiredManagedObjects();
            return this._vcInventoryNodes;
        }
    }
}
