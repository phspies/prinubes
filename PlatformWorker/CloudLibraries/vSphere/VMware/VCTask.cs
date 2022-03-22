using PlatformWorker.VMware.Interfaces;
using System;
using Prinubes.vCenterSDK;
using System.Threading.Tasks;

namespace PlatformWorker.VMware
{
    internal class VCTask : VCManagedItem, IVimTask, IVimManagedItem
    {
        private string _description;
        private DateTime? _completeTime;
        private TaskInfoState _state;

        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value;
            }
        }

        public DateTime? CompleteTime
        {
            get
            {
                return this._completeTime;
            }
            set
            {
                this._completeTime = value;
            }
        }

        public TaskInfoState State
        {
            get
            {
                return this._state;
            }
            set
            {
                this._state = value;
            }
        }

        internal VCTask(IVimService vimService, ManagedObjectReference managedObject) : base(vimService, managedObject)
        {
        }

        internal VCTask(IVimService vimService, ManagedObjectReference managedObject, string description, DateTime? completeTime, TaskInfoState state) : base(vimService, managedObject)
        {
            this._description = description;
            this._completeTime = completeTime;
            this._state = state;
        }

        public async Task WaitForResultAsync(string op, VimClientlContext rstate)
        {
            object[] objArray = await WaitForValues(rstate, new string[3] { "info.state", "info.error", "info.progress" }, new string[1] { "state" }, new object[1][] { new object[2] { (object)TaskInfoState.success, (object)TaskInfoState.error } });
            if (objArray[0].Equals((object)TaskInfoState.success))
            {
            }
            else
            {
                if (objArray.Length > 1 && objArray[1] != null)
                {
                    throw new Exception(((LocalizedMethodFault)objArray[1]).localizedMessage);
                }
                   throw new Exception("WaitForResult: Unknown error returned by Vim");
            }
        }

        public async Task CancelAsync()
        {
            try
            {
                await this.VcService.Service.CancelTaskAsync(this.ManagedObject);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
