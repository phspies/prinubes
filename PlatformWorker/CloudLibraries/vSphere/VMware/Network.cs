using PlatformWorker.VMware.Interfaces;
using Prinubes.vCenterSDK;

namespace PlatformWorker.VMware
{
    public class Network : VCManagedItem, IVimNetwork, IVimManagedItem
    {
        public static string[] VCProperties = new string[1] { "name" };
        public static string[] VCPortgroupProperties = new string[2] { "name", "key" };
        private NetworkProperties _properties;

        public NetworkProperties Properties
        {
            get
            {
                return _properties;
            }
            set
            {
                _properties = value;
            }
        }

        public bool IsDistributed
        {
            get
            {
                return _properties.IsDistributed;
            }
        }

        public string PortgroupKey
        {
            get
            {
                if (!IsDistributed)
                    return string.Empty;
                if (string.IsNullOrEmpty(_properties.PortgroupKey))
                {
                    GetCommonPropertiesAsync(GetPropertiesAsync(VCPortgroupProperties).Result).Wait();
                }
                return _properties.PortgroupKey;
            }
        }

        internal Network(IVimService vimService, ManagedObjectReference managedObject)
          : base(vimService, managedObject)
        {
            if (managedObject.type == "DistributedVirtualPortgroup")
                _properties.IsDistributed = true;
            else
                _properties.IsDistributed = false;
        }

        public async Task<NetworkProperties> GetCommonPropertiesAsync()
        {
            Dictionary<string, object> dictionary = !IsDistributed ? await GetPropertiesAsync(Network.VCProperties) : await GetPropertiesAsync(Network.VCPortgroupProperties);
            _properties.Name = (string)dictionary["name"];
            if (dictionary.ContainsKey("key"))
                _properties.PortgroupKey = (string)dictionary["key"];
            Name = _properties.Name;
            return _properties;
        }

        public async Task GetCommonPropertiesAsync(Dictionary<string, object> properties)
        {
            _properties.Name = (string)properties["name"];
            if (IsDistributed && !properties.ContainsKey("key"))
            {
                properties = await GetPropertiesAsync(Network.VCPortgroupProperties);
                if (properties.ContainsKey("key"))
                    _properties.PortgroupKey = (string)properties["key"];
            }
            Name = _properties.Name;
        }

        public override string GetName()
        {
            if (string.IsNullOrEmpty(_properties.Name))
                GetCommonPropertiesAsync().Wait();
            return Name;
        }
    }
}
