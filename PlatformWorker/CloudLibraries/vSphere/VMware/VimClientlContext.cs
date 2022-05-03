namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    public abstract class VimClientlContext
    {
        public bool IsRetriableCall = true;

        public abstract int TimeoutSec { get; }

        public abstract bool IsVimClientStopping();
    }
}
