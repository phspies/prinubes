﻿using PlatformWorker.VMware.Interfaces;
using Prinubes.vCenterSDK;
using System.Threading.Tasks;

namespace PlatformWorker.VMware
{
    internal class Snapshot : VCManagedItem, IVimSnapshot, IVimManagedItem
    {
        internal Snapshot(IVimService service, ManagedObjectReference managedObject) : base(service, managedObject)
        {
        }

        public override Task<IVimManagedItem[]> GetChildrenAsync()
        {
            return null;
        }
    }
}
