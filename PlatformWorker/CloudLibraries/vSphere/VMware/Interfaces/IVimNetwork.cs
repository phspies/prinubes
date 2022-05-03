﻿namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware.Interfaces
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
