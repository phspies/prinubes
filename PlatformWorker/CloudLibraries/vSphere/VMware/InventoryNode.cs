using Prinubes.vCenterSDK;

namespace PlatformWorker.VMware
{
    public class InventoryNode
    {
        private string _name;
        private ManagedObjectReference _managedObject;
        private ManagedObjectReference _parent;
        private List<ManagedObjectReference> _children;

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        public ManagedObjectReference ManagedObject
        {
            get
            {
                return this._managedObject;
            }
            set
            {
                this._managedObject = value;
            }
        }

        public ManagedObjectReference Parent
        {
            get
            {
                return this._parent;
            }
            set
            {
                this._parent = value;
            }
        }

        public List<ManagedObjectReference> Children
        {
            get
            {
                return this._children;
            }
            set
            {
                this._children = value;
            }
        }

        public InventoryNode(string name, ManagedObjectReference managedObject, ManagedObjectReference parent)
        {
            this._name = name;
            this._managedObject = managedObject;
            this._parent = parent;
            this._children = new List<ManagedObjectReference>();
        }
    }
}
