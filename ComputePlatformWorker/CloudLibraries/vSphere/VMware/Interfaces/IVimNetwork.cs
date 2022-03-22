using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlatformWorker.VMware.Interfaces
{
    public interface IVimNetwork : IVimManagedItem
    {
        bool IsDistributed { get; }

        string PortgroupKey { get; }

        NetworkProperties Properties { get; set; }

        Task GetCommonPropertiesAsync(Dictionary<string, object> properties);

        Task<NetworkProperties> GetCommonPropertiesAsync();
    }
}
