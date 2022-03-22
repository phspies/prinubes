using System.Collections.Generic;
using Prinubes.vCenterSDK;

namespace PlatformWorker.VMware.Interfaces
{
  public interface IVimDatacenter : IVimManagedItem
  {
    DatacenterProperties DatacenterProperties { get; set; }

    void GetCommonProperties(Dictionary<string, object> properties);

    void GetCommonProperties();

    ManagedObjectReference GetVmFolder();
  }
}
