using PlatformWorker.VMware.Interfaces;
using Prinubes.vCenterSDK;

namespace PlatformWorker.VMware
{
    public class ResourcePool : VCManagedItem, IVimResourcePool, IVimManagedItem
    {
        private ManagedObjectReference _parent;

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

        internal ResourcePool(IVimService vimService, ManagedObjectReference managedObject)
          : base(vimService, managedObject)
        {
        }

        public override string GetName()
        {
            if (string.IsNullOrEmpty(this.Name))
                this.Name = (string)this.GetPropertiesAsync(new string[1] { "name" }).Result["name"];
            return this.Name;
        }

        public void GetCommonProperties(Dictionary<string, object> properties)
        {
            if (properties.ContainsKey("name"))
                this.Name = (string)properties["name"];
            if (!properties.ContainsKey("parent"))
                return;
            this.Parent = (ManagedObjectReference)properties["parent"];
        }
    }
}
