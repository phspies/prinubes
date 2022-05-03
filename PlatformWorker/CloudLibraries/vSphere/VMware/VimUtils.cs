namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    public class VimUtils
    {
        public static string BuildDiskName(string serverName, string diskName)
        {
            return serverName + "_" + diskName + ".vmdk";
        }

        public static string GetVolumeName(string volName)
        {
            return string.IsNullOrEmpty(volName) ? "[Local] " : "[" + volName + "] ";
        }

        public static string VmfsUrlToDir(string url)
        {
            return "/vmfs/volumes/" + Datastore.GetRemoteId(url);
        }
    }
}
