using Prinubes.vCenterSDK;

namespace PlatformWorker.VMware.Interfaces
{
    public interface IVimTask : IVimManagedItem
    {
        DateTime? CompleteTime { get; set; }

        string Description { get; set; }

        TaskInfoState State { get; set; }

        Task CancelAsync();

        Task WaitForResultAsync(string op, VimClientlContext rstate);
    }
}
