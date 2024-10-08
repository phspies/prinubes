﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Prinubes.vCenterSDK;

namespace PlatformWorker.VMware.Interfaces
{
    public interface IVimManagedItem
    {
        DateTime CurrentTime { get; }

        string Name { get; set; }

        ManagedObjectReference ManagedObject { get; set; }

        Task<IVimManagedItem[]> GetChildrenAsync();

        string GetName();

        Task<Dictionary<string, object>> GetPropertiesAsync(string[] properties);

        Task<object> GetPropertyAsync(string property);

        Task InitializeManagedObjectAsync();

        Task<object[]> WaitForValues(VimClientlContext rstate, string[] filterProps, string[] endWaitProps, object[][] expectedVals);
    }
}
